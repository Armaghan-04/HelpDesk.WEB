using FluentValidation;
using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Entities;

namespace HelpDesk.WEB.Validators
{
    public sealed class ChangeRoleRequestValidator : AbstractValidator<ChangeRoleRequest>
    {
        public ChangeRoleRequestValidator()
        {
            RuleFor(request => request.Role)
                .Must(role => Roles.All.Contains(role, StringComparer.Ordinal))
                .WithMessage("Choose a valid role.");
        }
    }
}
