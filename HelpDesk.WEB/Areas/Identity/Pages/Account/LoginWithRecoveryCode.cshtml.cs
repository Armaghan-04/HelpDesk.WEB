using HelpDesk.WEB.Areas.Identity.Pages;
using Microsoft.AspNetCore.Authorization;

namespace HelpDesk.WEB.Areas.Identity.Pages.Account
{
    // 2FA is not part of this application. This override shadows the default
    // Identity UI page so the route returns 404 instead of rendering a recovery prompt.
    [AllowAnonymous]
    public class LoginWithRecoveryCodeModel : RemovedPageModel
    {
    }
}
