using HelpDesk.WEB.Models.Enums;
using HelpDesk.WEB.Models.Policies;

namespace HelpDesk.WEB.Models.Entities
{
    public class Ticket : BaseEntity
    {
        private readonly List<TicketComment> _comments = [];

        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public TicketStatus Status { get; private set; }
        public TicketPriority Priority { get; private set; }

        public Guid DepartmentId { get; private set; }
        public Guid CreatedById { get; private set; }
        public Guid? AssignedToId { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? ResolvedAtUtc { get; private set; }
        public DateTime? ClosedAtUtc { get; private set; }

        public byte[] RowVersion { get; private set; } = [];

        public Department Department { get; private set; } = null!;
        public AppUser CreatedByUser { get; private set; } = null!;
        public AppUser? AssignedTo { get; private set; }
        public IReadOnlyCollection<TicketComment> Comments => _comments.AsReadOnly();

        private Ticket() { }

        public Ticket(string title, string description, TicketPriority priority, Guid departmentId, Guid createdById, DateTime utcNow)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            Title = title.Trim();
            Description = description.Trim();
            Priority = priority;
            DepartmentId = departmentId;
            CreatedById = createdById;
            Status = TicketStatus.Open;
            CreatedAtUtc = utcNow;
        }

        public void UpdateDetails(string title, string description, TicketPriority priority, Guid departmentId, DateTime utcNow)
        {
            EnsureStatus("edit", TicketPolicy.IsEditable(Status));
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(description);

            Title = title.Trim();
            Description = description.Trim();
            Priority = priority;
            DepartmentId = departmentId;
            Touch(utcNow);
        }

        public void Assign(Guid technicianId, DateTime utcNow)
        {
            EnsureStatus("assign", TicketPolicy.CanAssign(Status));

            AssignedToId = technicianId;
            if (Status is TicketStatus.Open)
                Status = TicketStatus.Assigned;

            Touch(utcNow);
        }

        public void Unassign(DateTime utcNow)
        {
            EnsureStatus("unassign", TicketPolicy.CanUnassign(Status));

            AssignedToId = null;
            Status = TicketStatus.Open;
            Touch(utcNow);
        }

        public void Start(DateTime utcNow)
        {
            EnsureStatus("start", TicketPolicy.CanStart(Status));
            if (AssignedToId is null)
                throw new DomainException("A ticket must be assigned before work can start.");

            Status = TicketStatus.InProgress;
            Touch(utcNow);
        }

        public void Resolve(DateTime utcNow)
        {
            EnsureStatus("resolve", TicketPolicy.CanResolve(Status));

            Status = TicketStatus.Resolved;
            ResolvedAtUtc = utcNow;
            Touch(utcNow);
        }

        public void Close(DateTime utcNow)
        {
            EnsureStatus("close", TicketPolicy.CanClose(Status));

            Status = TicketStatus.Closed;
            ClosedAtUtc = utcNow;
            Touch(utcNow);
        }

        public void Reopen(DateTime utcNow)
        {
            EnsureStatus("reopen", TicketPolicy.CanReopen(Status));

            Status = TicketStatus.Reopened;
            ResolvedAtUtc = null;
            ClosedAtUtc = null;
            Touch(utcNow);
        }

        public TicketComment AddComment(Guid authorId, string body, bool isInternal, DateTime utcNow)
        {
            if (!TicketPolicy.CanComment(Status))
                throw new DomainException("Cannot comment on a closed ticket.");

            var comment = new TicketComment(Id, authorId, body, isInternal, utcNow);
            _comments.Add(comment);
            return comment;
        }

        private void Touch(DateTime utcNow) => UpdatedAtUtc = utcNow;

        private void EnsureStatus(string action, bool allowed)
        {
            if (!allowed)
                throw new DomainException($"Cannot {action} a ticket that is {Status}.");
        }
    }
}
