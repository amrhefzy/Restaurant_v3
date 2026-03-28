using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Customers;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager,Cashier")]
public sealed class CustomersController : BranchScopedController
{
    private readonly ICustomerService _service;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public CustomersController(
        ICustomerService service,
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
        var customers = branchId == Guid.Empty
            ? Array.Empty<CustomerDto>()
            : await _service.GetByBranchAsync(branchId, cancellationToken);

        return View(customers);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = _localizer["NoBranchConfigured"].Value;
            return RedirectToAction(nameof(Index));
        }

        return View(new CreateCustomerDto { BranchId = branchId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto request, CancellationToken cancellationToken)
    {
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.CreateAsync(request, cancellationToken);
            TempData["Success"] = _localizer["CustomerCreatedSuccessfully"].Value;
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
            TempData["Error"] = _localizer["CustomerWasNotFound"].Value;
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateCustomerDto request, CancellationToken cancellationToken)
    {
        request.Id = id;
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.UpdateAsync(request, cancellationToken);
            TempData["Success"] = _localizer["CustomerUpdatedSuccessfully"].Value;
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
