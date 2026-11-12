namespace HelpDesk.WEB.Models.ViewModels
{
    public enum TicketDetailsOutcome
    {
        Success,
        NotFound,
        Forbidden
    }

    public sealed record TicketDetailsResult(TicketDetailsOutcome Outcome, TicketDetailsViewModel? Model = null);
}
