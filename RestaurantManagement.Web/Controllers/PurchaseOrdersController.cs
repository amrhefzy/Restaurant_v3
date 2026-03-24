using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.PurchaseOrders;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

public sealed class PurchaseOrdersController : BranchScopedController
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var purchaseOrders = branchId == Guid.Empty
            ? Array.Empty<PurchaseOrderListItemDto>()
            : await _service.GetRecentByBranchAsync(branchId, 50, cancellationToken);

        return View(purchaseOrders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        var purchaseOrder = await _service.GetByIdAsync(branchId, id, cancellationToken);
        if (purchaseOrder is null)
        {
            TempData["Error"] = "Purchase order was not found.";
            return RedirectToAction(nameof(Index));
        }

        return Ok(purchaseOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDraft(CreatePurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        request.BranchId = branchId;

        try
        {
            var purchaseOrder = await _service.CreateDraftAsync(request, cancellationToken);
            TempData["Success"] = "Purchase order draft created successfully.";
            return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        var request = new SubmitPurchaseOrderRequestDto
        {
            BranchId = branchId,
            PurchaseOrderId = id
        };

        try
        {
            var purchaseOrder = await _service.SubmitAsync(request, cancellationToken);
            TempData["Success"] = "Purchase order submitted successfully.";
            return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(Guid id, ReceivePurchaseOrderRequestDto request, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        request.BranchId = branchId;
        request.PurchaseOrderId = id;
        request.Items = request.Items
            .Where(x => x.PurchaseOrderItemId != Guid.Empty && x.ReceivedQuantity > 0)
            .ToArray();

        try
        {
            var purchaseOrder = await _service.ReceiveAsync(request, cancellationToken);
            TempData["Success"] = "Purchase order receipt saved successfully.";
            return RedirectToAction(nameof(Details), new { id = purchaseOrder.Id });
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
