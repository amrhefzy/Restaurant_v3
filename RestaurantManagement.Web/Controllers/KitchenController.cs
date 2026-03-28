using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager,Cashier")]
public sealed class KitchenController : BranchScopedController
{
    private readonly IKitchenService _kitchenService;

    public KitchenController(IKitchenService kitchenService, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _kitchenService = kitchenService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
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
        var branchId = await GetBranchIdAsync(cancellationToken);
        var ok = await _kitchenService.StartOrderAsync(branchId, salesOrderId, cancellationToken);
        if (!ok)
        {
            return BadRequest(new { success = false, message = "Order cannot be started." });
        }

        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ready([FromBody] Guid salesOrderId, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var ok = await _kitchenService.MarkReadyAsync(branchId, salesOrderId, cancellationToken);
        if (!ok)
        {
            return BadRequest(new { success = false, message = "Order cannot be marked ready." });
        }

        return Json(new { success = true });
    }
}
