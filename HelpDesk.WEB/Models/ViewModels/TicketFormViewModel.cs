using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed class TicketFormViewModel
    {
        public Guid? Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public Guid? DepartmentId { get; set; }

        public IReadOnlyList<DepartmentOptionDto> Departments { get; set; } = [];

        public bool IsEdit => Id.HasValue;
    }
}
