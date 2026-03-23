using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.ViewModels.Inventory;

namespace RestaurantManagement.Web.Controllers;

public sealed class InventoryController : BranchScopedController
{
    private readonly IInventoryService _service;

    public InventoryController(IInventoryService service, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
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
}
