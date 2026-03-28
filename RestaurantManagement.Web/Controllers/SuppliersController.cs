using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Suppliers;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;

namespace RestaurantManagement.Web.Controllers;

public sealed class SuppliersController : BranchScopedController
{
    private readonly ISupplierService _service;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SuppliersController(
        ISupplierService service,
        IStringLocalizer<SharedResource> localizer,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var suppliers = branchId == Guid.Empty
            ? Array.Empty<SupplierDto>()
            : await _service.GetByBranchAsync(branchId, cancellationToken);

        return View(suppliers);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = _localizer["NoBranchConfigured"];
            return RedirectToAction(nameof(Index));
        }

        return View(new CreateSupplierDto { BranchId = branchId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSupplierDto request, CancellationToken cancellationToken)
    {
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.CreateAsync(request, cancellationToken);
            TempData["Success"] = _localizer["SupplierCreatedSuccessfully"];
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var model = await _service.GetForEditAsync(branchId, id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = _localizer["SupplierWasNotFound"];
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateSupplierDto request, CancellationToken cancellationToken)
    {
        request.Id = id;
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.UpdateAsync(request, cancellationToken);
            TempData["Success"] = _localizer["SupplierUpdatedSuccessfully"];
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
