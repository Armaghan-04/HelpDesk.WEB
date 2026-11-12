using HelpDesk.WEB.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpDesk.WEB.Areas.Identity
{
    /// <summary>
    /// Short-circuits the default Identity UI password-recovery handlers when the
    /// feature is disabled. The pages are supplied by the Identity UI package, so a
    /// page filter enforces the setting without duplicating their handlers.
    /// </summary>
    public sealed class PasswordRecoveryDisabledFilter : IPageFilter
    {
        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var settings = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<AuthenticationSettings>>()
                .Value;

            if (!settings.PasswordRecovery.Enabled)
                context.Result = new NotFoundResult();
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }
    }
}
