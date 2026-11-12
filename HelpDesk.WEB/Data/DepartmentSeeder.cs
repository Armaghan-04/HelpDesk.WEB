using HelpDesk.WEB.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.WEB.Data
{
    /// <summary>
    /// Seeds a small, fixed set of departments. There is deliberately no CRUD yet: the existing
    /// Department entity keeps the door open for a management page without another migration.
    /// </summary>
    public static class DepartmentSeeder
    {
        private static readonly (string Name, string Description)[] Defaults =
        [
            ("IT Support", "Accounts, devices, software, and network issues."),
            ("Facilities", "Buildings, desks, access cards, and maintenance."),
            ("Human Resources", "Contracts, leave, payroll, and people questions.")
        ];

        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var logger = services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(typeof(DepartmentSeeder));

            var existingNames = await context.Departments
                .AsNoTracking()
                .Select(department => department.Name)
                .ToListAsync();

            var known = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var added = 0;

            foreach (var (name, description) in Defaults)
            {
                // Add returns false for a case-insensitive duplicate, so restarting is safe.
                if (!known.Add(name))
                    continue;

                context.Departments.Add(new Department(name, description));
                added++;
            }

            if (added == 0)
                return;

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} department(s).", added);
        }
    }
}
