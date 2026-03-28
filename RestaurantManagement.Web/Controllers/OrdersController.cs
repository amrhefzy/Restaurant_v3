using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Customers;
using RestaurantManagement.Application.DTOs.Orders;
using RestaurantManagement.Application.DTOs.Products;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Application.DTOs.Tables;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager,Cashier")]
public sealed class OrdersController : BranchScopedController
{
    private readonly IOrderService _service;
    private readonly ICustomerService _customerService;
    private readonly ITableService _tableService;
    private readonly IProductService _productService;

    public OrdersController(
        IOrderService service,
        ICustomerService customerService,
        ITableService tableService,
        IProductService productService,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
        _customerService = customerService;
        _tableService = tableService;
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var orders = branchId == Guid.Empty
            ? Array.Empty<OrderDto>()
            : await _service.GetRecentByBranchAsync(branchId, 50, cancellationToken);

        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        var model = new CreateOrderDto
        {
            BranchId = branchId,
            Items = new List<CreateOrderItemDto> { new() }
        };

        await LoadOptionsAsync(branchId, null, null, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderDto request, CancellationToken cancellationToken)
    {
        request.BranchId = await GetBranchIdAsync(cancellationToken);
        request.Items = request.Items.Where(x => x.ProductId != Guid.Empty && x.Quantity > 0).ToList();

        try
        {
            await _service.CreateAsync(request, cancellationToken);
            TempData["Success"] = "Order created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadOptionsAsync(request.BranchId, request.CustomerId, request.TableId, cancellationToken);
            if (request.Items.Count == 0)
            {
                request.Items = new List<CreateOrderItemDto> { new() };
            }

            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var model = await _service.GetForEditAsync(branchId, id, cancellationToken);
        if (model is null)
        {
            TempData["Error"] = "Order was not found.";
            return RedirectToAction(nameof(Index));
        }

        if (model.Items.Count == 0)
        {
            model.Items = new List<UpdateOrderItemDto> { new() };
        }

        await LoadOptionsAsync(branchId, model.CustomerId, model.TableId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateOrderDto request, CancellationToken cancellationToken)
    {
        request.Id = id;
        request.BranchId = await GetBranchIdAsync(cancellationToken);
        request.Items = request.Items.Where(x => x.ProductId != Guid.Empty && x.Quantity > 0).ToList();

        try
        {
            await _service.UpdateAsync(request, cancellationToken);
            TempData["Success"] = "Order updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadOptionsAsync(request.BranchId, request.CustomerId, request.TableId, cancellationToken);
            if (request.Items.Count == 0)
            {
                request.Items = new List<UpdateOrderItemDto> { new() };
            }

            return View(request);
        }
        catch (KeyNotFoundException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task LoadOptionsAsync(Guid branchId, Guid? selectedCustomerId, Guid? selectedTableId, CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetByBranchAsync(branchId, cancellationToken);
        var tables = await _tableService.GetByBranchAsync(branchId, cancellationToken);
        var products = await _productService.GetByBranchAsync(branchId, cancellationToken);

        ViewBag.Customers = new SelectList(customers, nameof(CustomerDto.Id), nameof(CustomerDto.Name), selectedCustomerId);
        ViewBag.Tables = new SelectList(tables, nameof(TableDto.Id), nameof(TableDto.TableNumber), selectedTableId);
        ViewBag.Products = new SelectList(products, nameof(ProductDto.Id), nameof(ProductDto.NameEn));
    }
}
