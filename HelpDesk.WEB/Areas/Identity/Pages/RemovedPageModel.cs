using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDesk.WEB.Areas.Identity.Pages
{
    /// <summary>
    /// Base page model for Identity features this application does not offer.
    /// Every request returns 404, so the feature is unreachable even by direct URL.
    /// </summary>
    public abstract class RemovedPageModel : PageModel
    {
        public IActionResult OnGet() => NotFound();

        public IActionResult OnPost() => NotFound();
    }
}
