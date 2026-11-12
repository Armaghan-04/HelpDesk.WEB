using HelpDesk.WEB.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.WEB.Data
{
    public static class IdentitySeeder
    {
        private const string BootstrapManagerSection = "BootstrapManager";

        public static async Task SeedRolesAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var logger = services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(IdentitySeeder));

            foreach (var roleName in Roles.All)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                    continue;

                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));

                if (result.Succeeded)
                {
                    logger.LogInformation("Seeded role {Role}.", roleName);
                }
                else
                {
                    logger.LogError(
                        "Failed to seed role {Role}: {Errors}",
                        roleName,
                        string.Join("; ", result.Errors.Select(error => error.Description)));
                }
            }
        }

        /// <summary>
        /// Creates the first Manager when none exists. Without this, a fresh database has no one
        /// who can open the Team page, so no account could ever be promoted.
        /// Credentials come from the "BootstrapManager" configuration section (user secrets or
        /// environment variables outside development).
        /// </summary>
        public static async Task SeedBootstrapManagerAsync(IServiceProvider services)
        {
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(IdentitySeeder));

            if ((await userManager.GetUsersInRoleAsync(Roles.Manager)).Count > 0)
                return;

            var section = configuration.GetSection(BootstrapManagerSection);
            string email = section["Email"]?.Trim() ?? string.Empty;
            string password = section["Password"] ?? string.Empty;
            string fullName = section["FullName"]?.Trim() is { Length: > 0 } configuredName
                ? configuredName
                : "Help Desk Manager";

            if (email.Length == 0 || password.Length == 0)
            {
                logger.LogWarning(
                    "No Manager account exists and {Section}:Email/Password are not configured. " +
                    "Set them to bootstrap the first manager.",
                    BootstrapManagerSection);
                return;
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new AppUser(fullName, email, departmentId: null, DateTime.UtcNow);
                var createResult = await userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    logger.LogError(
                        "Failed to create the bootstrap manager {Email}: {Errors}",
                        email,
                        Describe(createResult));
                    return;
                }

                logger.LogInformation("Created the bootstrap manager account {Email}.", email);
            }

            if (await userManager.IsInRoleAsync(user, Roles.Manager))
                return;

            var roleResult = await userManager.AddToRoleAsync(user, Roles.Manager);

            if (roleResult.Succeeded)
            {
                logger.LogInformation("Granted the Manager role to the bootstrap account {Email}.", email);
            }
            else
            {
                logger.LogError(
                    "Failed to grant the Manager role to {Email}: {Errors}",
                    email,
                    Describe(roleResult));
            }
        }

        private static string Describe(IdentityResult result)
            => string.Join("; ", result.Errors.Select(error => error.Description));
    }
}
