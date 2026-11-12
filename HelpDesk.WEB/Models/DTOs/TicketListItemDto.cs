using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.DTOs
{
    public sealed record TicketListItemDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public TicketStatus Status { get; init; }
        public TicketPriority Priority { get; init; }
        public string DepartmentName { get; init; } = string.Empty;
        public string CreatedByName { get; init; } = string.Empty;
        public string? AssignedToName { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }
}
