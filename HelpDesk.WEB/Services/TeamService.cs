using HelpDesk.WEB.Common.Errors;
using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Data;
using HelpDesk.WEB.Mappings;
using HelpDesk.WEB.Models;
using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.Enums;
using HelpDesk.WEB.Models.ViewModels;
using HelpDesk.WEB.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.WEB.Services
{
    public sealed class TeamService(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        TeamMapper mapper) : ITeamService
    {
        public async Task<TeamIndexViewModel> GetTeamAsync(
            string? search,
            string? role,
            bool? isActive,
            TeamSortOption sort,
            int page,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            IQueryable<AppUser> query = context.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(user =>
                    user.FullName.Contains(term) ||
                    (user.Email != null && user.Email.Contains(term)));
            }

            string? roleFilter = Roles.All.FirstOrDefault(candidate =>
                string.Equals(candidate, role, StringComparison.OrdinalIgnoreCase));

            if (roleFilter is not null)
            {
                Guid? roleId = await context.Roles
                    .AsNoTracking()
                    .Where(roleRow => roleRow.Name == roleFilter)
                    .Select(roleRow => (Guid?)roleRow.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (roleId.HasValue)
                {
                    Guid filterRoleId = roleId.Value;
                    query = query.Where(user =>
                        context.UserRoles.Any(userRole =>
                            userRole.UserId == user.Id &&
                            userRole.RoleId == filterRoleId));
                }
            }

            if (isActive.HasValue)
                query = query.Where(user => user.IsActive == isActive.Value);

            int totalCount = await query.CountAsync(cancellationToken);
            Pagination pagination = Pagination.Create(page, totalCount);

            List<TeamMemberDto> members = await mapper
                .ProjectMembers(
                    ApplySort(query, sort)
                        .Skip(pagination.SkipCount)
                        .Take(pagination.PageSize))
                .ToListAsync(cancellationToken);

            List<Guid> memberIds = members.Select(member => member.Id).ToList();
            var rolesByUser = new Dictionary<Guid, IReadOnlyList<string>>();

            if (memberIds.Count > 0)
            {
                var roleRows = await (
                    from userRole in context.UserRoles.AsNoTracking()
                    join roleRow in context.Roles on userRole.RoleId equals roleRow.Id
                    where memberIds.Contains(userRole.UserId)
                    select new { userRole.UserId, RoleName = roleRow.Name })
                    .ToListAsync(cancellationToken);

                rolesByUser = roleRows
                    .GroupBy(row => row.UserId)
                    .ToDictionary(
                        group => group.Key,
                        group => (IReadOnlyList<string>)group
                            .Select(row => row.RoleName ?? string.Empty)
                            .Where(name => name.Length > 0)
                            .OrderBy(name => name)
                            .ToList());
            }

            int activeManagerCount = await CountActiveManagersAsync(cancellationToken);

            return new TeamIndexViewModel
            {
                Search = search,
                Role = roleFilter,
                IsActive = isActive,
                Sort = sort,
                Pagination = pagination,
                ActiveManagerCount = activeManagerCount,
                CurrentUserId = currentUserId,
                Members = members
                    .Select(member => member with
                    {
                        Roles = rolesByUser.TryGetValue(member.Id, out var roles) ? roles : []
                    })
                    .ToList()
            };
        }

        private static IQueryable<AppUser> ApplySort(
            IQueryable<AppUser> query,
            TeamSortOption sort)
            => sort switch
            {
                TeamSortOption.NameDesc => query
                    .OrderByDescending(user => user.FullName)
                    .ThenBy(user => user.Email),
                TeamSortOption.EmailAsc => query
                    .OrderBy(user => user.Email)
                    .ThenBy(user => user.FullName),
                TeamSortOption.EmailDesc => query
                    .OrderByDescending(user => user.Email)
                    .ThenBy(user => user.FullName),
                TeamSortOption.JoinedDesc => query
                    .OrderByDescending(user => user.CreatedAtUtc)
                    .ThenBy(user => user.FullName),
                TeamSortOption.JoinedAsc => query
                    .OrderBy(user => user.CreatedAtUtc)
                    .ThenBy(user => user.FullName),
                _ => query
                    .OrderBy(user => user.FullName)
                    .ThenBy(user => user.Email)
            };

        public async Task<IReadOnlyList<AssignableUserDto>> GetAssignableUsersAsync(
            CancellationToken cancellationToken = default)
        {
            List<Guid> roleIds = await context.Roles
                .AsNoTracking()
                .Where(role => role.Name == Roles.Technician || role.Name == Roles.Manager)
                .Select(role => role.Id)
                .ToListAsync(cancellationToken);

            return await mapper
                .ProjectAssignableUsers(
                    context.Users
                        .AsNoTracking()
                        .Where(user =>
                            user.IsActive &&
                            context.UserRoles.Any(userRole =>
                                userRole.UserId == user.Id &&
                                roleIds.Contains(userRole.RoleId)))
                        .OrderBy(user => user.FullName))
                .ToListAsync(cancellationToken);
        }

        public async Task<Result<string>> ChangeRoleAsync(
            Guid targetUserId,
            Guid actingUserId,
            string role,
            CancellationToken cancellationToken = default)
        {
            if (!Roles.All.Contains(role, StringComparer.Ordinal))
                return Result<string>.Failure(UserErrors.InvalidRole);

            AppUser? user = await FindUserAsync(targetUserId);
            if (user is null)
                return Result<string>.Failure(UserErrors.NotFound);

            if (user.Id == actingUserId)
                return Result<string>.Failure(UserErrors.CannotChangeOwnRole);

            IList<string> currentRoles = await userManager.GetRolesAsync(user);

            if (currentRoles.Count == 1 &&
                string.Equals(currentRoles[0], role, StringComparison.Ordinal))
            {
                return Result<string>.Success($"{user.FullName} is already a {role}.");
            }

            if (currentRoles.Contains(Roles.Manager) &&
                !string.Equals(role, Roles.Manager, StringComparison.Ordinal))
            {
                // Only active managers count, and the target is excluded so an inactive
                // manager can still be demoted without tripping the guard.
                int otherActiveManagers = await CountActiveManagersAsync(
                    cancellationToken,
                    excludingUserId: user.Id);

                if (otherActiveManagers == 0)
                {
                    return Result<string>.Failure(UserErrors.LastActiveManagerRoleChange);
                }
            }

            if (currentRoles.Count > 0)
            {
                IdentityResult removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    throw new InvalidOperationException(Describe(removeResult));
            }

            IdentityResult addResult = await userManager.AddToRoleAsync(user, role);
            if (!addResult.Succeeded)
            {
                // Put the original roles back so the account is never left without a role.
                if (currentRoles.Count > 0)
                    await userManager.AddToRolesAsync(user, currentRoles);

                throw new InvalidOperationException(Describe(addResult));
            }

            await userManager.UpdateSecurityStampAsync(user);

            return Result<string>.Success($"{user.FullName} is now a {role}.");
        }

        public async Task<Result> ActivateAsync(
            Guid targetUserId,
            Guid actingUserId,
            CancellationToken cancellationToken = default)
        {
            AppUser? user = await FindUserAsync(targetUserId);
            if (user is null)
                return Result.Failure(UserErrors.NotFound);

            // Idempotent: activating an already-active account is a no-op.
            if (user.IsActive)
                return Result.Success();

            user.Activate();
            await UpdateUserAsync(user);

            return Result.Success();
        }

        public async Task<Result> DeactivateAsync(
            Guid targetUserId,
            Guid actingUserId,
            CancellationToken cancellationToken = default)
        {
            AppUser? user = await FindUserAsync(targetUserId);
            if (user is null)
                return Result.Failure(UserErrors.NotFound);

            // Idempotent: deactivating an already-inactive account is a no-op.
            if (!user.IsActive)
                return Result.Success();

            if (user.Id == actingUserId)
                return Result.Failure(UserErrors.CannotDeactivateSelf);

            IList<string> roles = await userManager.GetRolesAsync(user);

            if (roles.Contains(Roles.Manager))
            {
                int otherActiveManagers = await CountActiveManagersAsync(
                    cancellationToken,
                    excludingUserId: user.Id);

                if (otherActiveManagers == 0)
                {
                    return Result.Failure(UserErrors.LastActiveManagerDeactivation);
                }
            }

            // Deactivation never deletes the account or clears roles. Ticket, comment,
            // and history rows keep their existing user references.
            user.Deactivate();
            await UpdateUserAsync(user);

            // Invalidate existing cookies at the next security-stamp validation.
            IdentityResult stampResult = await userManager.UpdateSecurityStampAsync(user);
            if (!stampResult.Succeeded)
                throw new InvalidOperationException(Describe(stampResult));

            return Result.Success();
        }

        private async Task<AppUser?> FindUserAsync(Guid userId)
            => await userManager.FindByIdAsync(userId.ToString());

        private async Task UpdateUserAsync(AppUser user)
        {
            IdentityResult result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException(Describe(result));
        }

        private async Task<int> CountActiveManagersAsync(
            CancellationToken cancellationToken,
            Guid? excludingUserId = null)
        {
            List<Guid> managerRoleIds = await context.Roles
                .AsNoTracking()
                .Where(role => role.Name == Roles.Manager)
                .Select(role => role.Id)
                .ToListAsync(cancellationToken);

            IQueryable<AppUser> managers = context.Users
                .AsNoTracking()
                .Where(user =>
                    user.IsActive &&
                    context.UserRoles.Any(userRole =>
                        userRole.UserId == user.Id &&
                        managerRoleIds.Contains(userRole.RoleId)));

            if (excludingUserId.HasValue)
                managers = managers.Where(user => user.Id != excludingUserId.Value);

            return await managers.CountAsync(cancellationToken);
        }

        private static string Describe(IdentityResult result)
            => string.Join(" ", result.Errors.Select(error => error.Description));
    }
}
