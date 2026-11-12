using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.WEB.Data
{
    /// <summary>
    /// Creates the predefined portfolio-demo accounts. The section is disabled by default and
    /// each account is created only when it is missing; existing accounts are never modified.
    /// Credentials come from the "DemoAccounts" configuration section (environment variables or
    /// user secrets), so nothing sensitive is committed to the repository.
    /// </summary>
    public static class DemoAccountSeeder
    {
        private const string SectionName = "DemoAccounts";

        private static readonly (string Key, string Role, string DefaultFullName)[] Accounts =
        [
            ("Employee", Roles.Employee, "Demo Employee"),
            ("Technician", Roles.Technician, "Demo Technician"),
            ("Manager", Roles.Manager, "Demo Manager")
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(SectionName);

            if (!section.GetValue<bool>("Enabled"))
                return;

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var logger = services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DemoAccountSeeder));

            foreach (var (key, role, defaultFullName) in Accounts)
            {
                await SeedAccountAsync(userManager, logger, section, key, role, defaultFullName);
            }
        }

        private static async Task SeedAccountAsync(
            UserManager<AppUser> userManager,
            ILogger logger,
            IConfigurationSection section,
            string key,
            string role,
            string defaultFullName)
        {
            var account = section.GetSection(key);
            string email = account["Email"]?.Trim() ?? string.Empty;
            string password = account["Password"] ?? string.Empty;

            if (email.Length == 0 || password.Length == 0)
            {
                logger.LogWarning(
                    "{Section}:{Key}:Email/Password are not configured; skipping the demo {Role} account.",
                    SectionName,
                    key,
                    role);
                return;
            }

            string fullName = account["FullName"]?.Trim() is { Length: > 0 } configuredName
                ? configuredName
                : defaultFullName;

            var existing = await userManager.FindByEmailAsync(email);

            if (existing is not null)
            {
                // Never reset an existing account's password, profile, or roles on later startups.
                if (!existing.IsActive)
                {
                    logger.LogWarning(
                        "The existing demo {Role} account {Email} is inactive; it was left unchanged.",
                        role,
                        email);
                }

                if (!await userManager.IsInRoleAsync(existing, role))
                {
                    logger.LogWarning(
                        "The existing demo {Role} account {Email} does not have its expected role; it was left unchanged.",
                        role,
                        email);
                }

                return;
            }

            var user = new AppUser(fullName, email, departmentId: null, DateTime.UtcNow);
            var createResult = await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                logger.LogError(
                    "Failed to create the demo {Role} account {Email}: {Errors}",
                    role,
                    email,
                    Describe(createResult));
                return;
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (roleResult.Succeeded)
            {
                logger.LogInformation("Created the demo {Role} account {Email}.", role, email);
                return;
            }

            logger.LogError(
                "Failed to grant the {Role} role to the demo account {Email}: {Errors}",
                role,
                email,
                Describe(roleResult));

            // Remove the incomplete account so the next startup can retry it cleanly.
            var deleteResult = await userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                logger.LogError(
                    "Failed to remove the incomplete demo account {Email}: {Errors}",
                    email,
                    Describe(deleteResult));
            }
        }

        private static string Describe(IdentityResult result)
            => string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
