using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.DTOs
{
    public sealed record TicketDetailsDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public TicketStatus Status { get; init; }
        public TicketPriority Priority { get; init; }

        public Guid DepartmentId { get; init; }
        public string DepartmentName { get; init; } = string.Empty;

        public Guid CreatedById { get; init; }
        public string CreatedByName { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }

        public Guid? AssignedToId { get; init; }
        public string? AssignedToName { get; init; }

        public DateTime? UpdatedAtUtc { get; init; }
        public DateTime? ResolvedAtUtc { get; init; }
        public DateTime? ClosedAtUtc { get; init; }

        public IReadOnlyList<TicketCommentDto> Comments { get; init; } = [];
        public IReadOnlyList<TicketHistoryDto> History { get; init; } = [];
    }
}
