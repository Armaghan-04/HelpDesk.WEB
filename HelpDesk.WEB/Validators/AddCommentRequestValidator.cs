using FluentValidation;
using HelpDesk.WEB.Common.Constraints;
using HelpDesk.WEB.Models.DTOs;

namespace HelpDesk.WEB.Validators
{
    public sealed class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
    {
        public AddCommentRequestValidator()
        {
            RuleFor(request => request.Body)
                .NotEmpty().WithMessage("Write a comment before posting.")
                .MaximumLength(DomainConstraints.TicketComment.BodyMaxLength)
                .WithMessage("Comments cannot be longer than {MaxLength} characters.");
        }
    }
}
