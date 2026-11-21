using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.WEB.Data
{
    /// <summary>
    /// A public, ready-to-use demo account. The credentials are intentionally committed
    /// because these are public portfolio accounts.
    /// </summary>
    public sealed record DemoAccount(string FullName, string Email, string Password, string Role);

    /// <summary>
    /// Creates the predefined demo accounts. Existing accounts are never modified.
    /// </summary>
    public static class DemoAccountSeeder
    {
        public static readonly IReadOnlyList<DemoAccount> Accounts =
        [
            new("Demo Employee", "demoemployee@helpdesk.com", "Pass@123", Roles.Employee),
            new("Demo Technician", "demotechnician@helpdesk.com", "Pass@123", Roles.Technician),
            new("Demo Manager", "demomanager@helpdesk.com", "Pass@123", Roles.Manager)
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var logger = services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DemoAccountSeeder));

            foreach (var account in Accounts)
            {
                if (await userManager.FindByEmailAsync(account.Email) is not null)
                    continue;

                var user = new AppUser(account.FullName, account.Email, departmentId: null, DateTime.UtcNow);
                var createResult = await userManager.CreateAsync(user, account.Password);

                if (!createResult.Succeeded)
                {
                    logger.LogError(
                        "Failed to create the demo {Role} account {Email}: {Errors}",
                        account.Role,
                        account.Email,
                        Describe(createResult));
                    continue;
                }

                var roleResult = await userManager.AddToRoleAsync(user, account.Role);

                if (!roleResult.Succeeded)
                {
                    logger.LogError(
                        "Failed to grant the {Role} role to the demo account {Email}: {Errors}",
                        account.Role,
                        account.Email,
                        Describe(roleResult));

                    await userManager.DeleteAsync(user);
                    continue;
                }

                logger.LogInformation("Created the demo {Role} account {Email}.", account.Role, account.Email);
            }
        }

        private static string Describe(IdentityResult result)
            => string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
