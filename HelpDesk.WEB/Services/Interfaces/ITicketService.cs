using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Enums;
using HelpDesk.WEB.Models.ViewModels;

namespace HelpDesk.WEB.Services.Interfaces
{
    public interface ITicketService
    {
        /// <summary>
        /// Returns one page of the ticket list visible to <paramref name="viewer"/>
        /// with all role-based filters already applied.
        /// </summary>
        Task<TicketIndexViewModel> GetIndexAsync(
            TicketViewer viewer,
            TicketStatus? status,
            TicketPriority? priority,
            Guid? departmentId,
            string? search,
            int page,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a ticket with the actions the viewer is allowed to perform.
        /// </summary>
        Task<TicketDetailsResult> GetDetailsAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default);

        Task<TicketFormViewModel> GetCreateFormAsync(
            CancellationToken cancellationToken = default);

        Task<TicketFormViewModel> PrepareFormAsync(
            TicketFormViewModel form,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a ticket and returns its id, or the business rule that blocked it.
        /// </summary>
        Task<Result<Guid>> CreateAsync(
            Guid actingUserId,
            TicketFormViewModel form,
            CancellationToken cancellationToken = default);

        Task<TicketFormResult> GetEditFormAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an editable ticket, or reports why it cannot be updated.
        /// </summary>
        Task<Result> UpdateDetailsAsync(
            Guid id,
            TicketViewer viewer,
            TicketFormViewModel form,
            CancellationToken cancellationToken = default);

        Task<Result> AssignAsync(
            Guid id,
            TicketViewer viewer,
            Guid technicianId,
            CancellationToken cancellationToken = default);

        Task<Result> UnassignAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default);

        Task<Result> StartAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default);

        Task<Result> ResolveAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default);

        Task<Result> CloseAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default);

        Task<Result> ReopenAsync(
            Guid id,
            TicketViewer viewer,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a comment, or reports why the ticket cannot receive one.
        /// </summary>
        Task<Result> AddCommentAsync(
            Guid id,
            TicketViewer viewer,
            AddCommentRequest request,
            CancellationToken cancellationToken = default);
    }
}
