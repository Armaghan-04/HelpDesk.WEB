using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.DTOs
{
    /// <summary>
    /// Optional filters applied when listing tickets. All filters are combined with AND.
    /// Visibility rules (e.g. Employee sees only own tickets) are applied by the caller.
    /// </summary>
    public sealed class TicketFilterDto
    {
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? AssignedToId { get; set; }
        public Guid? CreatedById { get; set; }

        /// <summary>Free-text search over the ticket title.</summary>
        public string? Search { get; set; }
    }
}
