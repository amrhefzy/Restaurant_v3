using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class ReportsController : BranchScopedController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _reportService = reportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Reports";
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            return View(new RestaurantManagement.Application.DTOs.Reports.ReportOverviewDto());
        }

        var fromUtc = DateTime.UtcNow.Date.AddDays(-29);
        var overview = await _reportService.GetOverviewAsync(branchId, fromUtc, DateTime.UtcNow, cancellationToken);
        return View(overview);
    }

    [HttpGet]
    public async Task<IActionResult> SummaryJson(int days = 30, CancellationToken cancellationToken = default)
    {
        days = days switch { <= 7 => 7, <= 30 => 30, _ => 90 };
        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId == Guid.Empty)
        {
            return Json(new { success = true, data = new RestaurantManagement.Application.DTOs.Reports.ReportOverviewDto() });
        }

        var fromUtc = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var overview = await _reportService.GetOverviewAsync(branchId, fromUtc, DateTime.UtcNow, cancellationToken);
        return Json(new { success = true, data = overview });
    }
}
