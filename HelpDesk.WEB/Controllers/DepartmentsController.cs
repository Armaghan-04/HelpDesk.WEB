using HelpDesk.WEB.Common.Errors;
using HelpDesk.WEB.Common.Results;
using HelpDesk.WEB.Models.Entities;
using HelpDesk.WEB.Models.ViewModels;
using HelpDesk.WEB.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.WEB.Controllers
{
    [Authorize(Roles = Roles.Manager)]
    public class DepartmentsController(IDepartmentService departmentService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
            => View(new DepartmentIndexViewModel
            {
                Departments = await departmentService.GetAllAsync(cancellationToken)
            });

        [HttpGet]
        public IActionResult Create() => View(new DepartmentFormViewModel());

        [HttpPost]
        public async Task<IActionResult> Create(
            DepartmentFormViewModel form,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(form);

            Result result = await departmentService.CreateAsync(form, cancellationToken);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Department '{form.Name.Trim()}' created.";
                return RedirectToAction(nameof(Index));
            }

            return HandleFormFailure(form, result);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
        {
            DepartmentFormViewModel? form = await departmentService.GetEditFormAsync(
                id,
                cancellationToken);

            return form is null ? NotFound() : View(form);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(
            Guid id,
            DepartmentFormViewModel form,
            CancellationToken cancellationToken)
        {
            form.Id = id;

            if (!ModelState.IsValid)
                return View(form);

            Result result = await departmentService.UpdateAsync(id, form, cancellationToken);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Department '{form.Name.Trim()}' updated.";
                return RedirectToAction(nameof(Index));
            }

            return HandleFormFailure(form, result);
        }

        [HttpPost]
        public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
        {
            Result result = await departmentService.ActivateAsync(id, cancellationToken);

            if (result.IsFailure)
                return RedirectWithFailure(result);

            TempData["Success"] = "Department activated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
        {
            Result result = await departmentService.DeactivateAsync(id, cancellationToken);

            if (result.IsFailure)
                return RedirectWithFailure(result);

            TempData["Success"] = "Department deactivated. Existing tickets keep their department.";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult HandleFormFailure(DepartmentFormViewModel form, Result result)
        {
            if (result.Error == DepartmentErrors.NotFound)
                return NotFound();

            ModelState.AddModelError(nameof(form.Name), result.Error.Description);
            return View(form);
        }

        private IActionResult RedirectWithFailure(Result result)
        {
            if (result.Error == DepartmentErrors.NotFound)
                return NotFound();

            TempData["Error"] = result.Error.Description;
            return RedirectToAction(nameof(Index));
        }
    }
}
