using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.ViewModels.Dashboard;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class DashboardController : BranchScopedController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService, IRepository<Branch> branchRepository)
        : base(branchRepository)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var businessDateUtc = DateTime.UtcNow;
        var branchId = await GetBranchIdAsync(cancellationToken);

        if (branchId == Guid.Empty)
        {
            TempData["Error"] = "No active branch context is available.";
            return View(new DashboardPageViewModel
            {
                BusinessDateUtc = businessDateUtc
            });
        }

        var summary = await _dashboardService.GetSummaryAsync(branchId, businessDateUtc, cancellationToken);

        return View(new DashboardPageViewModel
        {
            BusinessDateUtc = businessDateUtc,
            Summary = summary
        });
    }
}
