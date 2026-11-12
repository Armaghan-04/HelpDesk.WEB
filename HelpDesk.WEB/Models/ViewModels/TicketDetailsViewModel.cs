using HelpDesk.WEB.Models.DTOs;

namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed class TicketDetailsViewModel
    {
        public TicketDetailsDto Ticket { get; init; } = null!;

        public bool IsStaff { get; init; }

        public bool CanEdit { get; init; }
        public bool CanAssign { get; init; }
        public bool CanUnassign { get; init; }
        public bool CanStart { get; init; }
        public bool CanResolve { get; init; }
        public bool CanClose { get; init; }
        public bool CanReopen { get; init; }
        public bool CanComment { get; init; }

        public IReadOnlyList<AssignableUserDto> Technicians { get; init; } = [];
    }
}
