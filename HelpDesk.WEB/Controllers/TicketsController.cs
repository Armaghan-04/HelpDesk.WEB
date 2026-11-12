using HelpDesk.WEB.Common.Errors;
using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Extensions;
using HelpDesk.WEB.Models;
using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.Enums;
using HelpDesk.WEB.Models.ViewModels;
using HelpDesk.WEB.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WEB.Controllers
{
    [Authorize]
    public class TicketsController(ITicketService ticketService) : Controller
    {
        private TicketViewer Viewer => new(
            User.GetUserId(),
            User.IsInRole(Roles.Manager),
            User.IsInRole(Roles.Technician));

        [HttpGet]
        public async Task<IActionResult> Index(
            TicketStatus? status,
            TicketPriority? priority,
            Guid? departmentId,
            string? search,
            int page,
            CancellationToken cancellationToken)
            => View(await ticketService.GetIndexAsync(
                Viewer,
                status,
                priority,
                departmentId,
                search,
                page,
                cancellationToken));

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            TicketDetailsResult result = await ticketService.GetDetailsAsync(
                id,
                Viewer,
                cancellationToken);

            return result.Outcome switch
            {
                TicketDetailsOutcome.NotFound => NotFound(),
                TicketDetailsOutcome.Forbidden => Forbid(),
                _ => View(result.Model!)
            };
        }

        [HttpGet]
        [Authorize(Roles = Roles.Employee)]
        public async Task<IActionResult> Create(CancellationToken cancellationToken)
            => View(await ticketService.GetCreateFormAsync(cancellationToken));

        [HttpPost]
        [Authorize(Roles = Roles.Employee)]
        public async Task<IActionResult> Create(
            TicketFormViewModel form,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(await ticketService.PrepareFormAsync(form, cancellationToken));

            Result<Guid> result = await ticketService.CreateAsync(
                Viewer.UserId,
                form,
                cancellationToken);

            if (result.IsFailure)
            {
                ModelState.AddModelError(string.Empty, result.Error.Description);
                return View(await ticketService.PrepareFormAsync(form, cancellationToken));
            }

            TempData["Success"] = "Ticket created. We'll let you know as it moves along.";
            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            TicketFormResult result = await ticketService.GetEditFormAsync(
                id,
                Viewer,
                cancellationToken);

            switch (result.Outcome)
            {
                case TicketFormOutcome.NotFound:
                    return NotFound();

                case TicketFormOutcome.Forbidden:
                    return Forbid();

                case TicketFormOutcome.NotEditable:
                    TempData["Error"] = "This ticket can no longer be edited in its current status.";
                    return RedirectToAction(nameof(Details), new { id });

                default:
                    return View(result.Form!);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            Guid id,
            TicketFormViewModel form,
            CancellationToken cancellationToken)
        {
            form.Id = id;

            if (!ModelState.IsValid)
                return View(await ticketService.PrepareFormAsync(form, cancellationToken));

            Result result = await ticketService.UpdateDetailsAsync(
                id,
                Viewer,
                form,
                cancellationToken);

            if (result.IsSuccess)
            {
                TempData["Success"] = "Ticket details updated.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (result.Error == TicketErrors.NotFound ||
                result.Error == TicketErrors.UserNotFound ||
                result.Error == TicketErrors.DepartmentNotFound)
                return NotFound();

            if (result.Error == TicketErrors.ForbiddenEdit)
                return Forbid();

            ModelState.AddModelError(string.Empty, result.Error.Description);
            return View(await ticketService.PrepareFormAsync(form, cancellationToken));
        }

        [HttpPost]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> Assign(
            Guid id,
            AssignTicketRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Choose a technician before assigning.";
                return RedirectToAction(nameof(Details), new { id });
            }

            IActionResult? failure = await ExecuteAsync(
                token => ticketService.AssignAsync(id, Viewer, request.TechnicianId, token),
                "Ticket assigned.",
                cancellationToken);

            return failure ?? RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize(Roles = Roles.Manager)]
        public async Task<IActionResult> Unassign(Guid id, CancellationToken cancellationToken)
        {
            IActionResult? failure = await ExecuteAsync(
                token => ticketService.UnassignAsync(id, Viewer, token),
                "Ticket returned to the queue.",
                cancellationToken);

            return failure ?? RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
        {
            IActionResult? failure = await ExecuteAsync(
                token => ticketService.StartAsync(id, Viewer, token),
                "Work started on this ticket.",
                cancellationToken);

            return failure ?? RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Resolve(Guid id, CancellationToken cancellationToken)
        {
            IActionResult? failure = await ExecuteAsync(
                token => ticketService.ResolveAsync(id, Viewer, token),
                "Ticket marked as resolved.",
                cancellationToken);

            return failure ?? RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
        {
            IActionResult? failure = await ExecuteAsync(
                token => ticketService.CloseAsync(id, Viewer, token),
                "Ticket closed. Thanks for confirming.",
                cancellationToken);

            return failure ?? RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Reopen(Guid id, CancellationToken cancellationToken)
        {
            IActionResult? failure = await ExecuteAsync(
                token => ticketService.ReopenAsync(id, Viewer, token),
                "Ticket reopened.",
                cancellationToken);

            return failure ?? RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(
            Guid id,
            AddCommentRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                    ?? "Write a comment before posting.";
                return RedirectToAction(nameof(Details), new { id });
            }

            IActionResult? failure = await ExecuteAsync(
                token => ticketService.AddCommentAsync(id, Viewer, request, token),
                "Comment posted.",
                cancellationToken);

            return failure ?? RedirectToAction(nameof(Details), new { id });
        }

        private async Task<IActionResult?> ExecuteAsync(
            Func<CancellationToken, Task<Result>> action,
            string successMessage,
            CancellationToken cancellationToken)
        {
            Result result = await action(cancellationToken);

            if (result.IsSuccess)
            {
                TempData["Success"] = successMessage;
                return null;
            }

            if (result.Error == TicketErrors.ForbiddenAction ||
                result.Error == TicketErrors.ForbiddenComment)
                return Forbid();

            TempData["Error"] = result.Error.Description;
            return null;
        }
    }
}
