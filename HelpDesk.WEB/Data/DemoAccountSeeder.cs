using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.WEB.Data
{
    /// <summary>
    /// Creates the predefined demo accounts. The credentials are intentionally committed
    /// because these are public portfolio accounts; existing accounts are never modified.
    /// </summary>
    public static class DemoAccountSeeder
    {
        private static readonly (string FullName, string Email, string Password, string Role)[] Accounts =
        [
            ("Demo Employee", "demoemployee@helpdesk.com", "Pass@123", Roles.Employee),
            ("Demo Technician", "demotechnician@helpdesk.com", "Pass@123", Roles.Technician),
            ("Demo Manager", "demomanager@helpdesk.com", "Pass@123", Roles.Manager)
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var logger = services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DemoAccountSeeder));

            foreach (var (fullName, email, password, role) in Accounts)
            {
                if (await userManager.FindByEmailAsync(email) is not null)
                    continue;

                var user = new AppUser(fullName, email, departmentId: null, DateTime.UtcNow);
                var createResult = await userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    logger.LogError(
                        "Failed to create the demo {Role} account {Email}: {Errors}",
                        role,
                        email,
                        Describe(createResult));
                    continue;
                }

                var roleResult = await userManager.AddToRoleAsync(user, role);

                if (!roleResult.Succeeded)
                {
                    logger.LogError(
                        "Failed to grant the {Role} role to the demo account {Email}: {Errors}",
                        role,
                        email,
                        Describe(roleResult));

                    await userManager.DeleteAsync(user);
                    continue;
                }

                logger.LogInformation("Created the demo {Role} account {Email}.", role, email);
            }
        }

        private static string Describe(IdentityResult result)
            => string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
