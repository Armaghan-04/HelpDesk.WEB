using HelpDesk.WEB.Common.Errors;
using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Data;
using HelpDesk.WEB.Mappings;
using HelpDesk.WEB.Models;
using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.Enums;
using HelpDesk.WEB.Models.Policies;
using HelpDesk.WEB.Models.ViewModels;
using HelpDesk.WEB.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.WEB.Services
{
    public sealed class TicketService(
        ApplicationDbContext context,
        TicketMapper mapper,
        IDepartmentService departmentService,
        ITeamService teamService) : ITicketService
    {
        public async Task<TicketIndexViewModel> GetIndexAsync(
            TicketViewer viewer,
            TicketStatus? status,
            TicketPriority? priority,
            Guid? departmentId,
            string? search,
            int page,
            CancellationToken cancellationToken = default)
        {
            TicketFilterDto filter = new()
            {
                Status = status,
                Priority = priority,
                Search = search
            };

            string scopeLabel;

            if (viewer.IsManager)
            {
                filter.DepartmentId = departmentId;
                scopeLabel = "All requests across the desk";
            }
            else if (viewer.IsTechnician)
            {
                filter.AssignedToId = viewer.UserId;
                departmentId = null;
                scopeLabel = "Requests assigned to you";
            }
            else
            {
                filter.CreatedById = viewer.UserId;
                departmentId = null;
                scopeLabel = "Your support requests";
            }

            IQueryable<Ticket> query = ApplyFilter(filter);
            int totalCount = await query.CountAsync(cancellationToken);
            Pagination pagination = Pagination.Create(page, totalCount);

            List<TicketListItemDto> tickets = await mapper
                .ProjectList(
                    query.OrderByDescending(t => t.CreatedAtUtc)
                        .ThenByDescending(t => t.Id)
                        .Skip(pagination.SkipCount)
                        .Take(pagination.PageSize))
                .ToListAsync(cancellationToken);

            return new TicketIndexViewModel
            {
                Tickets = tickets,
                Status = status,
                Priority = priority,
                DepartmentId = departmentId,
                Search = search,
                Departments = viewer.IsManager
                    ? await departmentService.GetActiveOptionsAsync(cancellationToken)
                    : [],
                CanFilterByDepartment = viewer.IsManager,
                ScopeLabel = scopeLabel,
                Pagination = pagination
            };
        }

        public async Task<TicketDetailsResult> GetDetailsAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default)
        {
            TicketDetailsDto? ticket = await GetByIdAsync(
                id,
                includeInternalComments: viewer.IsStaff,
                cancellationToken);

            if (ticket is null)
                return new TicketDetailsResult(TicketDetailsOutcome.NotFound);

            TicketSnapshot snapshot = new(
                ticket.Status,
                ticket.CreatedById,
                ticket.AssignedToId);

            if (!TicketPolicy.CanView(snapshot, viewer))
                return new TicketDetailsResult(TicketDetailsOutcome.Forbidden);

            TicketDetailsViewModel model = new()
            {
                Ticket = ticket,
                IsStaff = viewer.IsStaff,
                CanEdit = TicketPolicy.CanEdit(snapshot, viewer),
                CanAssign = TicketPolicy.CanAssign(snapshot, viewer),
                CanUnassign = TicketPolicy.CanUnassign(snapshot, viewer),
                CanStart = TicketPolicy.CanStart(snapshot, viewer),
                CanResolve = TicketPolicy.CanResolve(snapshot, viewer),
                CanClose = TicketPolicy.CanClose(snapshot, viewer),
                CanReopen = TicketPolicy.CanReopen(snapshot, viewer),
                CanComment = TicketPolicy.CanComment(snapshot, viewer),
                Technicians = viewer.IsManager
                    ? await teamService.GetAssignableUsersAsync(cancellationToken)
                    : []
            };

            return new TicketDetailsResult(TicketDetailsOutcome.Success, model);
        }

        public async Task<TicketFormViewModel> GetCreateFormAsync(
            CancellationToken cancellationToken = default)
            => await PrepareFormAsync(new TicketFormViewModel(), cancellationToken);

        public async Task<TicketFormViewModel> PrepareFormAsync(
            TicketFormViewModel form,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(form);

            form.Departments = await departmentService.GetActiveOptionsAsync(cancellationToken);
            return form;
        }

        public async Task<Result<Guid>> CreateAsync(
            Guid actingUserId,
            TicketFormViewModel form,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(form);

            CreateTicketRequest request = mapper.ToCreateRequest(form);

            Error? userError = await FindUserErrorAsync(actingUserId, cancellationToken);
            if (userError is not null)
                return Result<Guid>.Failure(userError);

            Error? departmentError = await FindDepartmentErrorAsync(
                request.DepartmentId,
                cancellationToken);
            if (departmentError is not null)
                return Result<Guid>.Failure(departmentError);

            DateTime utcNow = DateTime.UtcNow;
            Ticket ticket = new(
                request.Title,
                request.Description,
                request.Priority,
                request.DepartmentId,
                actingUserId,
                utcNow);

            context.Tickets.Add(ticket);
            context.TicketHistories.Add(
                new TicketHistory(ticket.Id, actingUserId, fromStatus: null, toStatus: ticket.Status, utcNow));

            Result saveResult = await SaveChangesAsync(cancellationToken);

            return saveResult.IsSuccess
                ? Result<Guid>.Success(ticket.Id)
                : Result<Guid>.Failure(saveResult.Error);
        }

        public async Task<TicketFormResult> GetEditFormAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default)
        {
            TicketDetailsDto? ticket = await GetTicketAsync(id, cancellationToken);

            if (ticket is null)
                return new TicketFormResult(TicketFormOutcome.NotFound);

            TicketSnapshot snapshot = new(
                ticket.Status,
                ticket.CreatedById,
                ticket.AssignedToId);

            if (!TicketPolicy.CanEdit(snapshot, viewer))
            {
                return new TicketFormResult(
                    TicketPolicy.IsOwnerOrManager(snapshot, viewer)
                        ? TicketFormOutcome.NotEditable
                        : TicketFormOutcome.Forbidden);
            }

            return new TicketFormResult(
                TicketFormOutcome.Success,
                await PrepareFormAsync(mapper.ToForm(ticket), cancellationToken));
        }

        public async Task<Result> UpdateDetailsAsync(
            Guid id,
            TicketViewer viewer,
            TicketFormViewModel form,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(form);

            Error? userError = await FindUserErrorAsync(viewer.UserId, cancellationToken);
            if (userError is not null)
                return Result.Failure(userError);

            Ticket? ticket = await GetTrackedTicketAsync(id, cancellationToken);
            if (ticket is null)
                return Result.Failure(TicketErrors.NotFound);

            if (!TicketPolicy.IsOwnerOrManager(
                    new TicketSnapshot(ticket.Status, ticket.CreatedById, ticket.AssignedToId),
                    viewer))
            {
                return Result.Failure(TicketErrors.ForbiddenEdit);
            }

            UpdateTicketRequest request = mapper.ToUpdateRequest(form);

            Error? departmentError = await FindDepartmentErrorAsync(
                request.DepartmentId,
                cancellationToken);
            if (departmentError is not null)
                return Result.Failure(departmentError);

            try
            {
                ticket.UpdateDetails(
                    request.Title,
                    request.Description,
                    request.Priority,
                    request.DepartmentId,
                    DateTime.UtcNow);
            }
            catch (DomainException ex)
            {
                return Result.Failure(TicketErrors.RuleViolation(ex.Message));
            }

            return await SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> AssignAsync(
            Guid id,
            TicketViewer viewer,
            Guid technicianId,
            CancellationToken cancellationToken = default)
        {
            Error? technicianError = await FindTechnicianErrorAsync(technicianId, cancellationToken);
            if (technicianError is not null)
                return Result.Failure(technicianError);

            return await TransitionAsync(
                id,
                viewer,
                TicketAction.Assign,
                (ticket, utcNow) => ticket.Assign(technicianId, utcNow),
                cancellationToken);
        }

        public Task<Result> UnassignAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default)
            => TransitionAsync(
                id,
                viewer,
                TicketAction.Unassign,
                (ticket, utcNow) => ticket.Unassign(utcNow),
                cancellationToken);

        public Task<Result> StartAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default)
            => TransitionAsync(
                id,
                viewer,
                TicketAction.Start,
                (ticket, utcNow) => ticket.Start(utcNow),
                cancellationToken);

        public Task<Result> ResolveAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default)
            => TransitionAsync(
                id,
                viewer,
                TicketAction.Resolve,
                (ticket, utcNow) => ticket.Resolve(utcNow),
                cancellationToken);

        public Task<Result> CloseAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default)
            => TransitionAsync(
                id,
                viewer,
                TicketAction.Close,
                (ticket, utcNow) => ticket.Close(utcNow),
                cancellationToken);

        public Task<Result> ReopenAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default)
            => TransitionAsync(
                id,
                viewer,
                TicketAction.Reopen,
                (ticket, utcNow) => ticket.Reopen(utcNow),
                cancellationToken);

        public async Task<Result> AddCommentAsync(
            Guid id,
            TicketViewer viewer,
            AddCommentRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            Error? userError = await FindUserErrorAsync(viewer.UserId, cancellationToken);
            if (userError is not null)
                return Result.Failure(userError);

            Ticket? ticket = await GetTrackedTicketAsync(id, cancellationToken);
            if (ticket is null)
                return Result.Failure(TicketErrors.NotFound);

            TicketSnapshot snapshot = new(
                ticket.Status,
                ticket.CreatedById,
                ticket.AssignedToId);

            if (!TicketPolicy.CanView(snapshot, viewer))
                return Result.Failure(TicketErrors.ForbiddenComment);

            bool isInternal = viewer.IsStaff && request.IsInternal;
            TicketComment comment;

            try
            {
                comment = ticket.AddComment(
                    viewer.UserId,
                    request.Body,
                    isInternal,
                    DateTime.UtcNow);
            }
            catch (DomainException ex)
            {
                return Result.Failure(TicketErrors.RuleViolation(ex.Message));
            }

            context.TicketComments.Add(comment);

            return await SaveChangesAsync(cancellationToken);
        }

        private IQueryable<Ticket> ApplyFilter(TicketFilterDto filter)
        {
            IQueryable<Ticket> query = context.Tickets.AsNoTracking();

            if (filter.Status.HasValue)
                query = query.Where(t => t.Status == filter.Status.Value);

            if (filter.Priority.HasValue)
                query = query.Where(t => t.Priority == filter.Priority.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(t => t.DepartmentId == filter.DepartmentId.Value);

            if (filter.AssignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == filter.AssignedToId.Value);

            if (filter.CreatedById.HasValue)
                query = query.Where(t => t.CreatedById == filter.CreatedById.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                string term = filter.Search.Trim();
                query = query.Where(t => t.Title.Contains(term));
            }

            return query;
        }

        private async Task<TicketDetailsDto?> GetTicketAsync(
            Guid id,
            CancellationToken cancellationToken)
            => await mapper
                .ProjectDetails(
                    context.Tickets
                        .AsNoTracking()
                        .Where(candidate => candidate.Id == id))
                .FirstOrDefaultAsync(cancellationToken);

        private async Task<TicketDetailsDto?> GetByIdAsync(
            Guid id,
            bool includeInternalComments,
            CancellationToken cancellationToken)
        {
            TicketDetailsDto? ticket = await GetTicketAsync(id, cancellationToken);

            if (ticket is null) return null;

            List<TicketCommentDto> comments = await mapper
                .ProjectComments(
                    context.TicketComments
                        .AsNoTracking()
                        .Where(comment =>
                            comment.TicketId == id &&
                            (includeInternalComments || !comment.IsInternal))
                        .OrderBy(comment => comment.CreatedAtUtc))
                .ToListAsync(cancellationToken);

            List<TicketHistoryDto> history = await mapper
                .ProjectHistory(
                    context.TicketHistories
                        .AsNoTracking()
                        .Where(entry => entry.TicketId == id)
                        .OrderBy(entry => entry.ChangedAtUtc))
                .ToListAsync(cancellationToken);

            return ticket with { Comments = comments, History = history };
        }

        private async Task<Result> TransitionAsync(
            Guid id,
            TicketViewer viewer,
            TicketAction action,
            Action<Ticket, DateTime> transition,
            CancellationToken cancellationToken)
        {
            Error? userError = await FindUserErrorAsync(viewer.UserId, cancellationToken);
            if (userError is not null)
                return Result.Failure(userError);

            Ticket? ticket = await GetTrackedTicketAsync(id, cancellationToken);
            if (ticket is null)
                return Result.Failure(TicketErrors.NotFound);

            Error? forbiddenError = FindForbiddenActionError(action, ticket, viewer);
            if (forbiddenError is not null)
                return Result.Failure(forbiddenError);

            TicketStatus previousStatus = ticket.Status;
            DateTime utcNow = DateTime.UtcNow;

            try
            {
                transition(ticket, utcNow);
            }
            catch (DomainException ex)
            {
                return Result.Failure(TicketErrors.RuleViolation(ex.Message));
            }

            if (ticket.Status != previousStatus)
            {
                context.TicketHistories.Add(
                    new TicketHistory(ticket.Id, viewer.UserId, previousStatus, ticket.Status, utcNow));
            }

            return await SaveChangesAsync(cancellationToken);
        }

        private async Task<Ticket?> GetTrackedTicketAsync(Guid id, CancellationToken cancellationToken)
            => await context.Tickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        private async Task<Error?> FindUserErrorAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .AsNoTracking()
                .Where(candidate => candidate.Id == userId)
                .Select(candidate => new { candidate.IsActive })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                return TicketErrors.UserNotFound;

            return user.IsActive ? null : TicketErrors.UserInactive;
        }

        private async Task<Error?> FindDepartmentErrorAsync(
            Guid departmentId,
            CancellationToken cancellationToken)
        {
            var department = await context.Departments
                .AsNoTracking()
                .Where(candidate => candidate.Id == departmentId)
                .Select(candidate => new { candidate.Name, candidate.IsActive })
                .FirstOrDefaultAsync(cancellationToken);

            if (department is null)
                return TicketErrors.DepartmentNotFound;

            return department.IsActive
                ? null
                : TicketErrors.DepartmentInactive(department.Name);
        }

        private async Task<Error?> FindTechnicianErrorAsync(
            Guid technicianId,
            CancellationToken cancellationToken)
        {
            var technician = await context.Users
                .AsNoTracking()
                .Where(candidate => candidate.Id == technicianId)
                .Select(candidate => new
                {
                    candidate.FullName,
                    candidate.IsActive,
                    IsStaff = context.UserRoles.Any(userRole =>
                        userRole.UserId == candidate.Id &&
                        context.Roles.Any(role =>
                            role.Id == userRole.RoleId &&
                            (role.Name == Roles.Technician || role.Name == Roles.Manager)))
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (technician is null)
                return TicketErrors.TechnicianNotFound;

            if (!technician.IsActive)
                return TicketErrors.TechnicianInactive(technician.FullName);

            return technician.IsStaff ? null : TicketErrors.TechnicianNotStaff(technician.FullName);
        }

        private async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Failure(TicketErrors.ConcurrencyConflict);
            }
        }

        private static Error? FindForbiddenActionError(
            TicketAction action,
            Ticket ticket,
            TicketViewer viewer)
        {
            TicketSnapshot snapshot = new(
                ticket.Status,
                ticket.CreatedById,
                ticket.AssignedToId);

            bool allowed = action switch
            {
                TicketAction.Assign => TicketPolicy.CanAssign(snapshot, viewer),
                TicketAction.Unassign => TicketPolicy.CanUnassign(snapshot, viewer),
                TicketAction.Start => TicketPolicy.CanStart(snapshot, viewer),
                TicketAction.Resolve => TicketPolicy.CanResolve(snapshot, viewer),
                TicketAction.Close => TicketPolicy.CanClose(snapshot, viewer),
                TicketAction.Reopen => TicketPolicy.CanReopen(snapshot, viewer),
                _ => false
            };

            return allowed ? null : TicketErrors.ForbiddenAction;
        }

        private enum TicketAction
        {
            Assign,
            Unassign,
            Start,
            Resolve,
            Close,
            Reopen
        }
    }
}
