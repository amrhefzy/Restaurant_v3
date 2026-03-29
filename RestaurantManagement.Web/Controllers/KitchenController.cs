using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.Services;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager,Cashier")]
public sealed class KitchenController : BranchScopedController
{
    private readonly IKitchenService _kitchenService;
    private readonly ISettingsRuntimeService _settingsRuntime;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public KitchenController(
        IKitchenService kitchenService,
        ISettingsRuntimeService settingsRuntime,
        IStringLocalizer<SharedResource> localizer,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _kitchenService = kitchenService;
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

        ViewData["Title"] = "Kitchen";
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            return View(Array.Empty<RestaurantManagement.Application.DTOs.Kitchen.KitchenOrderCardDto>());
        }

        var activeOrders = await _kitchenService.GetActiveOrdersAsync(branchId, cancellationToken);
        return View(activeOrders);
    }

    [HttpGet]
    public async Task<IActionResult> Feed(CancellationToken cancellationToken)
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
            return Json(new { success = true, data = Array.Empty<RestaurantManagement.Application.DTOs.Kitchen.KitchenOrderCardDto>() });
        }

        var activeOrders = await _kitchenService.GetActiveOrdersAsync(branchId, cancellationToken);
        return Json(new { success = true, data = activeOrders });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start([FromBody] Guid salesOrderId, CancellationToken cancellationToken)
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
        var ok = await _kitchenService.StartOrderAsync(branchId, salesOrderId, cancellationToken);
        if (!ok)
        {
            return BadRequest(new { success = false, message = _localizer["KitchenOrderStartFailed"].Value });
        }

        return Json(new { success = true, message = _localizer["KitchenOrderStartedSuccessfully"].Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ready([FromBody] Guid salesOrderId, CancellationToken cancellationToken)
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
        var ok = await _kitchenService.MarkReadyAsync(branchId, salesOrderId, cancellationToken);
        if (!ok)
        {
            return BadRequest(new { success = false, message = _localizer["KitchenOrderReadyFailed"].Value });
        }

        return Json(new { success = true, message = _localizer["KitchenOrderMarkedReadySuccessfully"].Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete([FromBody] Guid salesOrderId, CancellationToken cancellationToken)
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
        var ok = await _kitchenService.CompleteOrderAsync(branchId, salesOrderId, cancellationToken);
        if (!ok)
        {
            return BadRequest(new { success = false, message = _localizer["KitchenOrderCompleteFailed"].Value });
        }

        return Json(new { success = true, message = _localizer["KitchenOrderCompletedSuccessfully"].Value });
    }
}
