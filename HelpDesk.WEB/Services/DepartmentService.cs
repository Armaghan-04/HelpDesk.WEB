using HelpDesk.WEB.Common.Errors;
using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Data;
using HelpDesk.WEB.Mappings;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.ViewModels;
using HelpDesk.WEB.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.WEB.Services
{
    public sealed class DepartmentService(ApplicationDbContext context, DepartmentMapper mapper) : IDepartmentService
    {
        public async Task<IReadOnlyList<DepartmentOptionDto>> GetActiveOptionsAsync(
            CancellationToken cancellationToken = default)
            => await mapper
                .ProjectOptions(
                    context.Departments
                        .AsNoTracking()
                        .Where(department => department.IsActive)
                        .OrderBy(department => department.Name))
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<DepartmentListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
            => await mapper
                .ProjectList(
                    context.Departments
                        .AsNoTracking()
                        .OrderByDescending(department => department.IsActive)
                        .ThenBy(department => department.Name))
                .ToListAsync(cancellationToken);

        public async Task<DepartmentFormViewModel?> GetEditFormAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            Department? department = await context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

            return department is null ? null : mapper.ToForm(department);
        }

        public async Task<Result> CreateAsync(
            DepartmentFormViewModel form,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(form);

            string name = form.Name.Trim();
            Error? duplicate = await FindDuplicateNameErrorAsync(name, excludeId: null, cancellationToken);
            if (duplicate is not null)
                return Result.Failure(duplicate);

            context.Departments.Add(new Department(name, form.Description));

            return await SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> UpdateAsync(
            Guid id,
            DepartmentFormViewModel form,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(form);

            Department? department = await FindTrackedAsync(id, cancellationToken);
            if (department is null)
                return Result.Failure(DepartmentErrors.NotFound);

            string name = form.Name.Trim();
            Error? duplicate = await FindDuplicateNameErrorAsync(name, id, cancellationToken);
            if (duplicate is not null)
                return Result.Failure(duplicate);

            department.Update(name, form.Description);

            return await SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> ActivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            Department? department = await FindTrackedAsync(id, cancellationToken);
            if (department is null)
                return Result.Failure(DepartmentErrors.NotFound);

            if (department.IsActive)
                return Result.Success();

            department.Activate();

            return await SaveChangesAsync(cancellationToken);
        }

        public async Task<Result> DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            Department? department = await FindTrackedAsync(id, cancellationToken);
            if (department is null)
                return Result.Failure(DepartmentErrors.NotFound);

            if (!department.IsActive)
                return Result.Success();

            // Deactivation never deletes a department. Existing tickets keep their FK.
            // The only existing rule that can block it is leaving the desk with no
            // active department to route new tickets to.
            int activeCount = await context.Departments.CountAsync(
                candidate => candidate.IsActive,
                cancellationToken);

            if (activeCount <= 1)
                return Result.Failure(DepartmentErrors.LastActiveDepartment);

            department.Deactivate();

            return await SaveChangesAsync(cancellationToken);
        }

        private async Task<Department?> FindTrackedAsync(Guid id, CancellationToken cancellationToken)
            => await context.Departments
                .FirstOrDefaultAsync(department => department.Id == id, cancellationToken);

        private async Task<Error?> FindDuplicateNameErrorAsync(
            string name,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            // SQL Server's default collation here is case-insensitive, and the unique
            // index mirrors that. Lower-casing keeps the service check explicit too.
            string normalized = name.ToLowerInvariant();

            IQueryable<Department> query = context.Departments
                .AsNoTracking()
                .Where(department => department.Name.ToLower() == normalized);

            if (excludeId.HasValue)
                query = query.Where(department => department.Id != excludeId.Value);

            return await query.AnyAsync(cancellationToken)
                ? DepartmentErrors.DuplicateName(name)
                : null;
        }

        private async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return Result.Success();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                // Covers the race between FindDuplicateNameErrorAsync and the unique index.
                return Result.Failure(DepartmentErrors.DuplicateNameConflict);
            }
        }
    }
}
