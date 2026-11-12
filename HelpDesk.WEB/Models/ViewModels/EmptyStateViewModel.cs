namespace HelpDesk.WEB.Models.ViewModels
{
    /// <summary>
    /// Model for the shared empty-state partial. Carries the icon, copy and an
    /// optional call-to-action link.
    /// </summary>
    public sealed record EmptyStateViewModel
    {
        public string IconId { get; init; } = "i-ticket";

        public string Title { get; init; } = "Nothing here yet";

        public string Message { get; init; } = string.Empty;

        public string? ActionText { get; init; }

        public string? ActionName { get; init; }

        public string? ActionController { get; init; }
    }
}
