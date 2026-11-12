using System.ComponentModel.DataAnnotations;
using HelpDesk.WEB.Common.Constraints;

namespace HelpDesk.WEB.Models.ViewModels
{
    public sealed class DepartmentFormViewModel
    {
        public Guid Id { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Enter a department name.")]
        [StringLength(
            DomainConstraints.Department.NameMaxLength,
            MinimumLength = DomainConstraints.Department.NameMinLength,
            ErrorMessage = "The name must be between {2} and {1} characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(
            DomainConstraints.Department.DescriptionMaxLength,
            ErrorMessage = "The description cannot exceed {1} characters.")]
        public string? Description { get; set; }

        public bool IsEdit => Id != Guid.Empty;
    }
}
