using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.Entities
{
    public class TicketHistory : BaseEntity
    {
        public Guid TicketId { get; private set; }
        public Guid ChangedById { get; private set; }
        public TicketStatus? FromStatus { get; private set; }
        public TicketStatus ToStatus { get; private set; }
        public DateTime ChangedAtUtc { get; private set; }

        public Ticket Ticket { get; private set; } = null!;
        public AppUser ChangedBy { get; private set; } = null!;

        private TicketHistory() { }

        internal TicketHistory(Guid ticketId, Guid changedById, TicketStatus? fromStatus, TicketStatus toStatus, DateTime utcNow)
        {
            TicketId = ticketId;
            ChangedById = changedById;
            FromStatus = fromStatus;
            ToStatus = toStatus;
            ChangedAtUtc = utcNow;
        }
    }
}
