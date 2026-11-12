using HelpDesk.WEB.Models.Enums;

namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed class TeamIndexViewModel
    {
        public IReadOnlyList<TeamMemberDto> Members { get; init; } = [];
        public string? Search { get; init; }
        public string? Role { get; init; }
        public bool? IsActive { get; init; }
        public TeamSortOption Sort { get; init; } = TeamSortOption.NameAsc;

        public int ActiveManagerCount { get; init; }
        public Guid CurrentUserId { get; init; }

        public Pagination Pagination { get; init; } = new();
    }
}
