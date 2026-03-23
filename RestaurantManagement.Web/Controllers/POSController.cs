using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.POS;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

public sealed class POSController : BranchScopedController
{
    private readonly IPosService _posService;

    public POSController(IPosService posService, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _posService = posService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "POS";
        var branchId = await GetBranchIdAsync(cancellationToken);

        if (branchId == Guid.Empty)
        {
            return View(new PosScreenDataDto());
        }

        var screenData = await _posService.GetScreenDataAsync(branchId, cancellationToken);
        return View(screenData);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout([FromBody] PosCheckoutRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            request.BranchId = await GetBranchIdAsync(cancellationToken);
            request.IsHold = false;

            var result = await _posService.ProcessOrderAsync(request, cancellationToken);
            return Json(new { success = true, data = result });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hold([FromBody] PosCheckoutRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            request.BranchId = await GetBranchIdAsync(cancellationToken);
            request.IsHold = true;

            var result = await _posService.ProcessOrderAsync(request, cancellationToken);
            return Json(new { success = true, data = result });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Held(Guid id, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var held = await _posService.GetHeldOrderAsync(branchId, id, cancellationToken);
        if (held is null)
        {
            return NotFound(new { success = false, message = "Held order not found." });
        }

        return Json(new { success = true, data = held });
    }

    [HttpGet]
    public async Task<IActionResult> StatusFeed(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            return Json(new { success = true, data = Array.Empty<PosOrderStatusDto>() });
        }

        var statuses = await _posService.GetActiveOrderStatusesAsync(branchId, cancellationToken);
        return Json(new { success = true, data = statuses });
    }
}
