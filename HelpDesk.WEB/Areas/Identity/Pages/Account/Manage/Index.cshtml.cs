using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDesk.WEB.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel(UserManager<AppUser> userManager) : PageModel
    {
        public string? Username { get; set; }

        public string? Email { get; set; }

        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
                return NotFound();

            Username = await userManager.GetUserNameAsync(user);
            Email = await userManager.GetEmailAsync(user);
            FullName = user.FullName;
            CreatedAtUtc = user.CreatedAtUtc;
            Roles = await userManager.GetRolesAsync(user);

            return Page();
        }
    }
}
