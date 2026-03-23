using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Customers;
using RestaurantManagement.Application.DTOs.Reservations;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Application.DTOs.Tables;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

public sealed class ReservationsController : BranchScopedController
{
    private readonly IReservationService _service;
    private readonly ICustomerService _customerService;
    private readonly ITableService _tableService;

    public ReservationsController(
        IReservationService service,
        ICustomerService customerService,
        ITableService tableService,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
        _customerService = customerService;
        _tableService = tableService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var reservations = branchId == Guid.Empty
            ? Array.Empty<ReservationDto>()
            : await _service.GetUpcomingByBranchAsync(branchId, cancellationToken);

        return View(reservations);
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

        var model = new CreateReservationDto
        {
            BranchId = branchId,
            ReservationAtUtc = DateTime.UtcNow.AddHours(1)
        };

        await LoadOptionsAsync(branchId, null, null, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReservationDto request, CancellationToken cancellationToken)
    {
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.CreateAsync(request, cancellationToken);
            TempData["Success"] = "Reservation created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadOptionsAsync(request.BranchId, request.CustomerId, request.TableId, cancellationToken);
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
            TempData["Error"] = "Reservation was not found.";
            return RedirectToAction(nameof(Index));
        }

        await LoadOptionsAsync(branchId, model.CustomerId, model.TableId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateReservationDto request, CancellationToken cancellationToken)
    {
        request.Id = id;
        request.BranchId = await GetBranchIdAsync(cancellationToken);

        try
        {
            await _service.UpdateAsync(request, cancellationToken);
            TempData["Success"] = "Reservation updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadOptionsAsync(request.BranchId, request.CustomerId, request.TableId, cancellationToken);
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

        ViewBag.Customers = new SelectList(customers, nameof(CustomerDto.Id), nameof(CustomerDto.Name), selectedCustomerId);
        ViewBag.Tables = new SelectList(tables, nameof(TableDto.Id), nameof(TableDto.TableNumber), selectedTableId);
    }
}
