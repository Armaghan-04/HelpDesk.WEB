using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.DTOs
{
    public sealed class UpdateTicketRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public Guid DepartmentId { get; set; }
    }
}
