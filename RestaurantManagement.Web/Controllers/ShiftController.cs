using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Shifts;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.Services;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager,Cashier")]
public sealed class ShiftController : BranchScopedController
{
    private readonly IShiftService _shiftService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ISettingsRuntimeService _settingsRuntime;

    public ShiftController(
        IShiftService shiftService,
        ICurrentUserService currentUserService,
        IStringLocalizer<SharedResource> localizer,
        ISettingsRuntimeService settingsRuntime,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _shiftService = shiftService;
        _currentUserService = currentUserService;
        _localizer = localizer;
        _settingsRuntime = settingsRuntime;
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

        var branchId = await GetBranchIdAsync(cancellationToken);
        var current = branchId == Guid.Empty
            ? null
            : await _shiftService.GetCurrentAsync(branchId, cancellationToken);

        return View(current);
    }

    [HttpGet]
    public async Task<IActionResult> Open(CancellationToken cancellationToken)
    {
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

        return View(new OpenShiftRequestDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(OpenShiftRequestDto request, CancellationToken cancellationToken)
    {
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

        try
        {
            request.BranchId = await GetBranchIdAsync(cancellationToken);
            request.OpenedByUserId = _currentUserService.UserId;
            request.OpenedByUserName = _currentUserService.UserName ?? "System";
            await _shiftService.OpenAsync(request, cancellationToken);
            TempData["Success"] = _localizer["ShiftOpenedSuccessfully"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Close(CancellationToken cancellationToken)
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
        var current = await _shiftService.GetCurrentAsync(branchId, cancellationToken);
        if (current is null)
        {
            TempData["Error"] = _localizer["NoOpenShiftAvailable"].Value;
            return RedirectToAction(nameof(Index));
        }

        var model = new CloseShiftRequestDto();
        ViewBag.CurrentShift = current;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(CloseShiftRequestDto request, CancellationToken cancellationToken)
    {
        var loginGuard = await GuardOperationalLoginRequirementAsync(
            _settingsRuntime,
            _localizer["OperationalLoginRequiredActionBlocked"].Value,
            cancellationToken);
        if (loginGuard is not null)
        {
            return loginGuard;
        }

        try
        {
            request.BranchId = await GetBranchIdAsync(cancellationToken);
            request.ClosedByUserId = _currentUserService.UserId;
            request.ClosedByUserName = _currentUserService.UserName ?? "System";
            await _shiftService.CloseAsync(request, cancellationToken);
            TempData["Success"] = _localizer["ShiftClosedSuccessfully"].Value;
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            var current = await _shiftService.GetCurrentAsync(request.BranchId, cancellationToken);
            ViewBag.CurrentShift = current;
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }
}
