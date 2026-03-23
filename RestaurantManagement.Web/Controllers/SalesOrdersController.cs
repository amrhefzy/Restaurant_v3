using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.SalesOrders;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

public sealed class SalesOrdersController : BranchScopedController
{
    private readonly ISalesOrderService _service;

    public SalesOrdersController(ISalesOrderService service, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var salesOrders = branchId == Guid.Empty
            ? Array.Empty<SalesOrderListItemDto>()
            : await _service.GetRecentByBranchAsync(branchId, 50, cancellationToken);

        return View(salesOrders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkPaid(Guid id, PaymentType paymentType, CancellationToken cancellationToken)
    {
        try
        {
            var branchId = await GetBranchIdAsync(cancellationToken);
            await _service.MarkAsPaidAsync(branchId, id, paymentType, cancellationToken);
            TempData["Success"] = "Payment recorded successfully.";
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var branchId = await GetBranchIdAsync(cancellationToken);
            await _service.CloseAsync(branchId, id, cancellationToken);
            TempData["Success"] = "Order closed successfully.";
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
