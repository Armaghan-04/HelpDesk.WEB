using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.Policies
{
    /// <summary>
    /// Single source of truth for ticket permissions. Every decision about who may
    /// perform an action, and in which status, lives here so the UI flags and the
    /// service authorization checks cannot drift apart.
    /// </summary>
    public static class TicketPolicy
    {
        public static bool IsEditable(TicketStatus status)
            => status is TicketStatus.Open
                or TicketStatus.Assigned
                or TicketStatus.InProgress
                or TicketStatus.Reopened;

        public static bool CanAssign(TicketStatus status)
            => status is TicketStatus.Open
                or TicketStatus.Assigned
                or TicketStatus.InProgress
                or TicketStatus.Reopened;

        public static bool CanUnassign(TicketStatus status)
            => status is TicketStatus.Assigned
                or TicketStatus.InProgress
                or TicketStatus.Reopened;

        public static bool CanStart(TicketStatus status)
            => status is TicketStatus.Assigned or TicketStatus.Reopened;

        public static bool CanResolve(TicketStatus status)
            => status is TicketStatus.InProgress;

        public static bool CanClose(TicketStatus status)
            => status is TicketStatus.Resolved;

        public static bool CanReopen(TicketStatus status)
            => status is TicketStatus.Resolved or TicketStatus.Closed;

        public static bool CanComment(TicketStatus status)
            => status is not TicketStatus.Closed;

        public static bool CanView(TicketSnapshot ticket, TicketViewer viewer)
            => viewer.IsStaff || ticket.CreatedById == viewer.UserId;

        public static bool CanComment(TicketSnapshot ticket, TicketViewer viewer)
            => CanComment(ticket.Status) && CanView(ticket, viewer);

        public static bool CanEdit(TicketSnapshot ticket, TicketViewer viewer)
            => IsEditable(ticket.Status) && IsOwnerOrManager(ticket, viewer);

        public static bool CanAssign(TicketSnapshot ticket, TicketViewer viewer)
            => viewer.IsManager && CanAssign(ticket.Status);

        public static bool CanUnassign(TicketSnapshot ticket, TicketViewer viewer)
            => viewer.IsManager
                && ticket.AssignedToId is not null
                && CanUnassign(ticket.Status);

        public static bool CanStart(TicketSnapshot ticket, TicketViewer viewer)
            => IsManagerOrAssignee(ticket, viewer) && CanStart(ticket.Status);

        public static bool CanResolve(TicketSnapshot ticket, TicketViewer viewer)
            => IsManagerOrAssignee(ticket, viewer) && CanResolve(ticket.Status);

        public static bool CanClose(TicketSnapshot ticket, TicketViewer viewer)
            => IsOwnerOrManager(ticket, viewer) && CanClose(ticket.Status);

        public static bool CanReopen(TicketSnapshot ticket, TicketViewer viewer)
            => IsOwnerOrManager(ticket, viewer) && CanReopen(ticket.Status);

        public static bool IsOwnerOrManager(TicketSnapshot ticket, TicketViewer viewer)
            => viewer.IsManager || ticket.CreatedById == viewer.UserId;

        private static bool IsManagerOrAssignee(TicketSnapshot ticket, TicketViewer viewer)
            => viewer.IsManager
                || (viewer.IsStaff && ticket.AssignedToId == viewer.UserId);
    }

    /// <summary>
    /// The ticket data permission checks need, independent of whether it came
    /// from a tracked entity or a read-only DTO.
    /// </summary>
    public readonly record struct TicketSnapshot(
        TicketStatus Status,
        Guid CreatedById,
        Guid? AssignedToId);
}
