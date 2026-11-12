using FluentValidation;
using HelpDesk.WEB.Common.Constraints;
using HelpDesk.WEB.Models.ViewModels;

namespace HelpDesk.WEB.Validators
{
    public sealed class TicketFormViewModelValidator : AbstractValidator<TicketFormViewModel>
    {
        public TicketFormViewModelValidator()
        {
            RuleFor(form => form.Title)
                .NotEmpty().WithMessage("Enter a title.")
                .Length(DomainConstraints.Ticket.TitleMinLength, DomainConstraints.Ticket.TitleMaxLength)
                .WithMessage("The title must be between {MinLength} and {MaxLength} characters.");

            RuleFor(form => form.Description)
                .NotEmpty().WithMessage("Describe the issue.")
                .Length(DomainConstraints.Ticket.DescriptionMinLength, DomainConstraints.Ticket.DescriptionMaxLength)
                .WithMessage("The description must be between {MinLength} and {MaxLength} characters.");

            RuleFor(form => form.DepartmentId)
                .Must(departmentId => departmentId.HasValue && departmentId.Value != Guid.Empty)
                .WithMessage("Choose a department.");

            RuleFor(form => form.Priority)
                .IsInEnum().WithMessage("Choose a valid priority.");
        }
    }
}
