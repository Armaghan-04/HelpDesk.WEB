namespace HelpDesk.WEB.Models.ViewModels
{
    /// <summary>
    /// Immutable paging state shared by list screens.
    /// </summary>
    public sealed record Pagination
    {
        public const int DefaultPageSize = 8;

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = DefaultPageSize;
        public int TotalCount { get; init; }

        public int TotalPages => PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public int FirstItemNumber => TotalCount == 0
            ? 0
            : ((Page - 1) * PageSize) + 1;

        public int LastItemNumber => Math.Min(Page * PageSize, TotalCount);

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;

        public int SkipCount => (Page - 1) * PageSize;

        public static Pagination Create(
            int page,
            int totalCount,
            int pageSize = DefaultPageSize)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            int currentPage = Math.Clamp(page, 1, Math.Max(totalPages, 1));

            return new Pagination
            {
                Page = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
