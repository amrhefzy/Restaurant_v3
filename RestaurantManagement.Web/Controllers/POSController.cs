using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.POS;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.Services;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager,Cashier")]
public sealed class POSController : BranchScopedController
{
    private readonly IPosService _posService;
    private readonly ISettingsRuntimeService _settingsRuntime;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public POSController(
        IPosService posService,
        ISettingsRuntimeService settingsRuntime,
        IStringLocalizer<SharedResource> localizer,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _posService = posService;
        _settingsRuntime = settingsRuntime;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

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
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

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
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

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
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

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
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            return Json(new { success = true, data = Array.Empty<PosOrderStatusDto>() });
        }

        var statuses = await _posService.GetActiveOrderStatusesAsync(branchId, cancellationToken);
        return Json(new { success = true, data = statuses });
    }
}
