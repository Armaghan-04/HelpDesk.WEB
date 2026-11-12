using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Models.ViewModels;

namespace HelpDesk.WEB.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<IReadOnlyList<DepartmentOptionDto>> GetActiveOptionsAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DepartmentListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<DepartmentFormViewModel?> GetEditFormAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Result> CreateAsync(
            DepartmentFormViewModel form,
            CancellationToken cancellationToken = default);

        Task<Result> UpdateAsync(
            Guid id,
            DepartmentFormViewModel form,
            CancellationToken cancellationToken = default);

        Task<Result> ActivateAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Result> DeactivateAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
