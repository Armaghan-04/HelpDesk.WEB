namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed class DepartmentIndexViewModel
    {
        public IReadOnlyList<DepartmentListItemDto> Departments { get; init; } = [];

        public int ActiveCount => Departments.Count(department => department.IsActive);
    }
}
