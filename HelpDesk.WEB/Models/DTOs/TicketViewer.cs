namespace HelpDesk.WEB.Models.DTOs
{
    /// <summary>
    /// Identifies who is performing an action so services can apply ticket rules.
    /// </summary>
    public sealed record TicketViewer(Guid UserId, bool IsManager, bool IsTechnician)
    {
        public bool IsStaff => IsManager || IsTechnician;
    }
}
