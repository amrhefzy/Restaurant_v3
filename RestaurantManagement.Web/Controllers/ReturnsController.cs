using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Returns;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class ReturnsController : BranchScopedController
{
    private readonly IReturnService _service;

    public ReturnsController(IReturnService service, IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var summary = branchId == Guid.Empty
            ? new ReturnSummaryDto()
            : await _service.GetSummaryAsync(branchId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, cancellationToken);

        return View(summary);
    }
}
