using FluentValidation;
using HelpDesk.WEB.Models.DTOs;

namespace HelpDesk.WEB.Validators
{
    public sealed class AssignTicketRequestValidator : AbstractValidator<AssignTicketRequest>
    {
        public AssignTicketRequestValidator()
        {
            RuleFor(request => request.TechnicianId)
                .NotEmpty().WithMessage("Choose a technician before assigning.");
        }
    }
}
