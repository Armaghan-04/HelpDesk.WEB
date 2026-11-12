using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace HelpDesk.WEB.Validation
{
    /// <summary>
    /// Keeps FluentValidation failures inside the normal MVC flow. Returning
    /// <c>null</c> lets the action run with an invalid <c>ModelState</c>, so
    /// controllers can redisplay the form or redirect with a TempData message
    /// instead of emitting the default JSON 400 response.
    /// </summary>
    public sealed class MvcValidationResultFactory : IFluentValidationAutoValidationResultFactory
    {
        public Task<IActionResult?> CreateActionResult(
            ActionExecutingContext context,
            ValidationProblemDetails validationProblemDetails,
            IDictionary<IValidationContext, ValidationResult> validationResults)
            => Task.FromResult<IActionResult?>(null);
    }
}
