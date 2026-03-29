using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Products;
using RestaurantManagement.Application.DTOs.PurchaseOrders;
using RestaurantManagement.Application.DTOs.Suppliers;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.Services;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class PurchaseOrdersController : BranchScopedController
{
    private readonly IPurchaseOrderService _service;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;
    private readonly ISettingsRuntimeService _settingsRuntime;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public PurchaseOrdersController(
        IPurchaseOrderService service,
        ISupplierService supplierService,
        IProductService productService,
        ISettingsRuntimeService settingsRuntime,
        IStringLocalizer<SharedResource> localizer,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
        _supplierService = supplierService;
        _productService = productService;
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
        var purchaseOrders = branchId == Guid.Empty
            ? Array.Empty<PurchaseOrderListItemDto>()
            : await _service.GetRecentByBranchAsync(branchId, 50, cancellationToken);

        return View(purchaseOrders);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var inventoryGuard = await GuardInventoryTrackingAsync(cancellationToken);
        if (inventoryGuard is not null)
        {
            return inventoryGuard;
        }

        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        await LoadOptionsAsync(branchId, null, cancellationToken);
        return View(new CreatePurchaseOrderDto
        {
            BranchId = branchId,
            Items = new List<CreatePurchaseOrderItemDto> { new() }
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var inventoryGuard = await GuardInventoryTrackingAsync(cancellationToken);
        if (inventoryGuard is not null)
        {
            return inventoryGuard;
        }

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

        return View(purchaseOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDraft(CreatePurchaseOrderDto request, CancellationToken cancellationToken)
    {
        var inventoryGuard = await GuardInventoryTrackingAsync(cancellationToken);
        if (inventoryGuard is not null)
        {
            return inventoryGuard;
        }

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
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadOptionsAsync(branchId, request.SupplierId, cancellationToken);
            if (request.Items.Count == 0)
            {
                request.Items = new List<CreatePurchaseOrderItemDto> { new() };
            }

            return View("Create", request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        var inventoryGuard = await GuardInventoryTrackingAsync(cancellationToken);
        if (inventoryGuard is not null)
        {
            return inventoryGuard;
        }

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
        var inventoryGuard = await GuardInventoryTrackingAsync(cancellationToken);
        if (inventoryGuard is not null)
        {
            return inventoryGuard;
        }

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

    private async Task<IActionResult?> GuardInventoryTrackingAsync(CancellationToken cancellationToken)
    {
        if (await _settingsRuntime.IsInventoryTrackingEnabledAsync(cancellationToken))
        {
            return null;
        }

        TempData["Error"] = _localizer["InventoryTrackingDisabledActionBlocked"].Value;
        return RedirectToAction("Index", "Dashboard");
    }

    private async Task LoadOptionsAsync(Guid branchId, Guid? selectedSupplierId, CancellationToken cancellationToken)
    {
        var suppliers = await _supplierService.GetByBranchAsync(branchId, cancellationToken);
        var products = await _productService.GetByBranchAsync(branchId, cancellationToken);

        ViewBag.Suppliers = new SelectList(suppliers, nameof(SupplierDto.Id), nameof(SupplierDto.Name), selectedSupplierId);
        ViewBag.Products = new SelectList(products, nameof(ProductDto.Id), nameof(ProductDto.NameEn));
    }
}
