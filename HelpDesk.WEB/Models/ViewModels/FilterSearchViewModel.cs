namespace HelpDesk.WEB.Models.ViewModels
{
    /// <summary>
    /// Model for the shared search column of a list-page filter bar.
    /// </summary>
    public sealed record FilterSearchViewModel
    {
        public string Label { get; init; } = "Search";

        public string Placeholder { get; init; } = "Search...";

        public string? Value { get; init; }
    }
}
