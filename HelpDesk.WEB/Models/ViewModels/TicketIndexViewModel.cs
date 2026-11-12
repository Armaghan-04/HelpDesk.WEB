using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed class TicketIndexViewModel
    {
        public IReadOnlyList<TicketListItemDto> Tickets { get; init; } = [];

        public TicketStatus? Status { get; init; }
        public TicketPriority? Priority { get; init; }
        public Guid? DepartmentId { get; init; }
        public string? Search { get; init; }

        public IReadOnlyList<DepartmentOptionDto> Departments { get; init; } = [];
        public bool CanFilterByDepartment { get; init; }
        public string ScopeLabel { get; init; } = "Your requests";

        public Pagination Pagination { get; init; } = new();
    }
}
