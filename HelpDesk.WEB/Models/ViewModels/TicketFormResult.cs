namespace HelpDesk.WEB.Models.ViewModels
{
    public enum TicketFormOutcome
    {
        Success,
        NotFound,
        Forbidden,
        NotEditable
    }

    public sealed record TicketFormResult(TicketFormOutcome Outcome, TicketFormViewModel? Form = null);
}
