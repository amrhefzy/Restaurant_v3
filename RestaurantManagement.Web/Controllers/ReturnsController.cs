using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Returns;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.Services;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class ReturnsController : BranchScopedController
{
    private readonly IReturnService _service;
    private readonly ISettingsRuntimeService _settingsRuntime;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ReturnsController(
        IReturnService service,
        ISettingsRuntimeService settingsRuntime,
        IStringLocalizer<SharedResource> localizer,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
        _settingsRuntime = settingsRuntime;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var inventoryGuard = await GuardInventoryTrackingAsync(cancellationToken);
        if (inventoryGuard is not null)
        {
            return inventoryGuard;
        }

        var branchId = await GetBranchIdAsync(cancellationToken);
        var summary = branchId == Guid.Empty
            ? new ReturnSummaryDto()
            : await _service.GetSummaryAsync(branchId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, cancellationToken);

        return View(summary);
    }

    private async Task<IActionResult?> GuardInventoryTrackingAsync(CancellationToken cancellationToken)
    {
        if (await _settingsRuntime.IsInventoryTrackingEnabledAsync(cancellationToken))
        {
            return null;
        }

        TempData["Error"] = _localizer["InventoryTrackingDisabledActionBlocked"].Value;
        return RedirectToAction("Index", "Dashboard");
    }
}
