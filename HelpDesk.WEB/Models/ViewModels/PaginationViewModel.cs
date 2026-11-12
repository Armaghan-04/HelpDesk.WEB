namespace HelpDesk.WEB.Models.ViewModels
{
    /// <summary>
    /// Model for the shared pagination partial. Carries the current paging state
    /// and the filter route values that every page link must preserve.
    /// </summary>
    public sealed record PaginationViewModel
    {
        public Pagination Pagination { get; init; } = new();

        public string Action { get; init; } = "Index";

        public string? Controller { get; init; }

        public IDictionary<string, string> RouteValues { get; init; }
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string AriaLabel { get; init; } = "Pagination";

        public string ItemNounSingular { get; init; } = "item";

        public string ItemNounPlural { get; init; } = "items";

        public string SummaryText => Pagination.TotalCount == 1
            ? $"Showing {Pagination.FirstItemNumber}–{Pagination.LastItemNumber} of {Pagination.TotalCount} {ItemNounSingular}"
            : $"Showing {Pagination.FirstItemNumber}–{Pagination.LastItemNumber} of {Pagination.TotalCount} {ItemNounPlural}";

        public static IDictionary<string, string> BuildRouteValues(
            params (string Key, string? Value)[] values)
        {
            var routeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (key, value) in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    routeValues[key] = value;
            }

            return routeValues;
        }
    }
}
