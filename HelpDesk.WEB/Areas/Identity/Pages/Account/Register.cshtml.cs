using System.ComponentModel.DataAnnotations;
using HelpDesk.WEB.Common.Constraints;
using HelpDesk.WEB.Configuration;
using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace HelpDesk.WEB.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IOptions<AuthenticationSettings> authenticationSettings) : PageModel
    {
        private readonly AuthenticationSettings _authenticationSettings = authenticationSettings.Value;

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (!_authenticationSettings.Registration.Enabled)
                return NotFound();

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            // Direct POST requests must be rejected too, not just the form page.
            if (!_authenticationSettings.Registration.Enabled)
                return NotFound();

            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return Page();

            // AppUser has no settable members, so it must be built through its constructor.
            var user = new AppUser(Input.FullName, Input.Email, departmentId: null, DateTime.UtcNow);

            var result = await userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                // New accounts always start at the lowest privilege level.
                // A manager can promote the account from the Team page.
                var roleResult = await userManager.AddToRoleAsync(user, Roles.Employee);

                if (!roleResult.Succeeded)
                {
                    await userManager.DeleteAsync(user);

                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return Page();
                }

                await signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(ReturnUrl);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

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
            [Required(ErrorMessage = "Tell us your name.")]
            [StringLength(
                DomainConstraints.User.FullNameMaxLength,
                MinimumLength = DomainConstraints.User.FullNameMinLength)]
            [Display(Name = "Full name")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Enter your email address.")]
            [EmailAddress(ErrorMessage = "That does not look like a valid email address.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Choose a password.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Use at least {2} characters.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare(nameof(Password), ErrorMessage = "The password and confirmation do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
