using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Tables;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager,Cashier")]
public sealed class TablesController : BranchScopedController
{
    private readonly ITableService _service;

    public TablesController(ITableService service, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var tables = branchId == Guid.Empty
            ? Array.Empty<TableDto>()
            : await _service.GetByBranchAsync(branchId, cancellationToken);

        return View(tables);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        return View(new CreateTableDto { BranchId = branchId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTableDto request, CancellationToken cancellationToken)
    {
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.CreateAsync(request, cancellationToken);
            TempData["Success"] = "Table created successfully.";
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
            TempData["Error"] = "Table was not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateTableDto request, CancellationToken cancellationToken)
    {
        request.Id = id;
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.UpdateAsync(request, cancellationToken);
            TempData["Success"] = "Table updated successfully.";
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
