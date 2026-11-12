using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.ViewModels;
using Riok.Mapperly.Abstractions;

namespace HelpDesk.WEB.Mappings
{
    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public partial class DepartmentMapper
    {
        public partial IQueryable<DepartmentOptionDto> ProjectOptions(IQueryable<Department> source);

        public partial IQueryable<DepartmentListItemDto> ProjectList(IQueryable<Department> source);

        public partial DepartmentFormViewModel ToForm(Department source);
    }
}
