namespace HelpDesk.WEB.Models.Entities
{
    public class TicketComment : BaseEntity
    {
        public Guid TicketId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Body { get; private set; } = string.Empty;
        public bool IsInternal { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        public Ticket Ticket { get; private set; } = null!;
        public AppUser Author { get; private set; } = null!;

        private TicketComment() { }

        internal TicketComment(Guid ticketId, Guid authorId, string body, bool isInternal, DateTime utcNow)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            TicketId = ticketId;
            AuthorId = authorId;
            Body = body.Trim();
            IsInternal = isInternal;
            CreatedAtUtc = utcNow;
        }

        public void Edit(string body, DateTime utcNow)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            Body = body.Trim();
            UpdatedAtUtc = utcNow;
        }
    }
}
