using HelpDesk.WEB.Common.Errors;
using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Extensions;
using HelpDesk.WEB.Models;
using HelpDesk.WEB.Models.DTOs;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.Enums;
using HelpDesk.WEB.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WEB.Controllers
{
    [Authorize(Roles = Roles.Manager)]
    public class UsersController(ITeamService teamService) : Controller
    {
        private Guid CurrentUserId => User.GetUserId();

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? role,
            bool? isActive,
            TeamSortOption sort,
            int page,
            CancellationToken cancellationToken)
            => View(await teamService.GetTeamAsync(
                search,
                role,
                isActive,
                sort,
                page,
                CurrentUserId,
                cancellationToken));

        [HttpPost]
        public async Task<IActionResult> ChangeRole(
            Guid id,
            ChangeRoleRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = UserErrors.InvalidRole.Description;
                return RedirectToAction(nameof(Index));
            }

            Result<string> result = await teamService.ChangeRoleAsync(
                id,
                CurrentUserId,
                request.Role,
                cancellationToken);

            return result.IsSuccess
                ? RedirectWithSuccess(result.Value)
                : RedirectWithFailure(result);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        {
            Result result = await teamService.ActivateAsync(
                id,
                CurrentUserId,
                cancellationToken);

            return result.IsSuccess
                ? RedirectWithSuccess("User activated. They can sign in again.")
                : RedirectWithFailure(result);
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            Result result = await teamService.DeactivateAsync(
                id,
                CurrentUserId,
                cancellationToken);

            return result.IsSuccess
                ? RedirectWithSuccess("User deactivated. Existing ticket history is preserved.")
                : RedirectWithFailure(result);
        }

        private IActionResult RedirectWithSuccess(string message)
        {
            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        private IActionResult RedirectWithFailure(Result result)
        {
            if (result.Error == UserErrors.NotFound)
                return NotFound();

            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }
    }
}
