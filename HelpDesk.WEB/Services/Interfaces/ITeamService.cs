using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Enums;
using HelpDesk.WEB.Models.ViewModels;

namespace HelpDesk.WEB.Services.Interfaces
{
    public interface ITeamService
    {
        Task<TeamIndexViewModel> GetTeamAsync(
            string? search,
            string? role,
            bool? isActive,
            TeamSortOption sort,
            int page,
            Guid currentUserId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AssignableUserDto>> GetAssignableUsersAsync(
            CancellationToken cancellationToken = default);

        Task<Result<string>> ChangeRoleAsync(
            Guid targetUserId,
            Guid actingUserId,
            string role,
            CancellationToken cancellationToken = default);

        Task<Result> ActivateAsync(
            Guid targetUserId,
            Guid actingUserId,
            CancellationToken cancellationToken = default);

        Task<Result> DeactivateAsync(
            Guid targetUserId,
            Guid actingUserId,
            CancellationToken cancellationToken = default);
    }
}
