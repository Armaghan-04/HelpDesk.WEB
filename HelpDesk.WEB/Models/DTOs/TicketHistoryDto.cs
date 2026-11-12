using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.DTOs
{
    public sealed record TicketHistoryDto
    {
        public string ChangedByName { get; init; } = string.Empty;
        public TicketStatus? FromStatus { get; init; }
        public TicketStatus ToStatus { get; init; }
        public DateTime ChangedAtUtc { get; init; }
    }
}
