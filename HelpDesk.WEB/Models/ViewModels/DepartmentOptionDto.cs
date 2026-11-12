namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed record DepartmentOptionDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
