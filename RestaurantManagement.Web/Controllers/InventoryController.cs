using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Inventory;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.ViewModels.Inventory;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class InventoryController : BranchScopedController
{
    private readonly IInventoryService _service;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public InventoryController(
        IInventoryService service,
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

        if (branchId == Guid.Empty)
        {
            return View(new InventoryOverviewViewModel());
        }

        var lowStock = await _service.GetLowStockItemsAsync(branchId, cancellationToken);
        var movements = await _service.GetRecentMovementsAsync(branchId, 100, cancellationToken);

        return View(new InventoryOverviewViewModel
        {
            LowStockItems = lowStock,
            RecentMovements = movements
        });
    }

    [HttpGet]
    public async Task<IActionResult> StockOnHand(bool lowStockOnly = false, CancellationToken cancellationToken = default)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = _localizer["NoBranchConfigured"].Value;
            return RedirectToAction(nameof(Index));
        }

        var stock = await _service.GetStockOnHandAsync(branchId, cancellationToken);
        if (lowStockOnly)
        {
            stock = stock.Where(x => x.OnHandQuantity <= x.ReorderLevel).ToArray();
        }

        return Ok(stock);
    }

    [HttpGet]
    public async Task<IActionResult> MovementHistory(
        Guid? productId,
        InventoryMovementType? movementType,
        DateTime? fromUtc,
        DateTime? toUtc,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = _localizer["NoBranchConfigured"].Value;
            return RedirectToAction(nameof(Index));
        }

        var request = new InventoryMovementHistoryRequestDto
        {
            BranchId = branchId,
            ProductId = productId,
            MovementType = movementType,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Skip = skip,
            Take = take
        };

        var history = await _service.GetMovementHistoryAsync(request, cancellationToken);
        return Ok(history);
    }
}
