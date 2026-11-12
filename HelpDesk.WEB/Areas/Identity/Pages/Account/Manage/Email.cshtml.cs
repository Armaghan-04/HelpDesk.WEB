using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDesk.WEB.Areas.Identity.Pages.Account.Manage
{
    // Email is intentionally read-only in this application. The default Identity
    // change-email flow is redirected back to the profile page.
    [Authorize]
    public class EmailModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("./Index");

        public IActionResult OnPost() => RedirectToPage("./Index");

        public IActionResult OnPostChangeEmail() => RedirectToPage("./Index");

        public IActionResult OnPostSendVerificationEmail() => RedirectToPage("./Index");
    }
}
