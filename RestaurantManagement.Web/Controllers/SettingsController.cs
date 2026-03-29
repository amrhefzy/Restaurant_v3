using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.ViewModels.Settings;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class SettingsController : BranchScopedController
{
    public SettingsController(IRepository<Branch> branchRepository) : base(branchRepository)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);

        var model = new SettingsPageViewModel
        {
            ActiveBranchId = branchId == Guid.Empty ? null : branchId,
            RuntimeNotes = "Linux runtime active, SQL Server runs via Docker, auth and role-aware navigation are enabled."
        };

        return View(model);
    }
}
