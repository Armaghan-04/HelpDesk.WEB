namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed record AssignableUserDto
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
    }
}
