using System.ComponentModel.DataAnnotations;
using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HelpDesk.WEB.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel(
        SignInManager<AppUser> signInManager,
        UserManager<AppUser> userManager,
        ILogger<LoginModel> logger) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // Validate credentials and existing Identity checks without issuing a cookie.
            AppUser? user = await userManager.FindByNameAsync(Input.Email);

            if (user is not null)
            {
                var result = await signInManager.CheckPasswordSignInAsync(
                    user,
                    Input.Password,
                    lockoutOnFailure: true);

                if (result.Succeeded && !user.IsActive)
                {
                    // Same generic message as a bad password so account status is not revealed.
                    logger.LogWarning("Inactive user attempted to sign in.");
                    ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
                    return Page();
                }

                if (result.Succeeded)
                {
                    // Only now is the authentication cookie created.
                    await signInManager.SignInAsync(user, Input.RememberMe);
                    logger.LogInformation("User logged in.");
                    return RedirectToLocal(ReturnUrl);
                }

                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
            }

            ModelState.AddModelError(string.Empty, "Email or password is incorrect.");
            return Page();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public sealed class InputModel
        {
            [Required(ErrorMessage = "Enter your email address.")]
            [EmailAddress(ErrorMessage = "That does not look like a valid email address.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Enter your password.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Keep me signed in")]
            public bool RememberMe { get; set; }
        }
    }
}
