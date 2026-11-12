namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed record TeamMemberDto
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public IReadOnlyList<string> Roles { get; init; } = [];
        public string? DepartmentName { get; init; }
        public DateTime CreatedAtUtc { get; init; }

        public string PrimaryRole => Roles.Count > 0 ? Roles[0] : "None";
    }
}
