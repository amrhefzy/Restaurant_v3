using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.ViewModels.Dashboard;

namespace RestaurantManagement.Web.Controllers;

public sealed class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IRepository<Branch> _branchRepository;

    public DashboardController(IDashboardService dashboardService, IRepository<Branch> branchRepository)
    {
        _dashboardService = dashboardService;
        _branchRepository = branchRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var businessDateUtc = DateTime.UtcNow;
        var branchId = await _branchRepository
            .Query()
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (branchId == Guid.Empty)
        {
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
