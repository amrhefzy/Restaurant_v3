using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using FluentValidation;

namespace RestaurantManagement.Web.Controllers.Base;

public abstract class BranchScopedController : Controller
{
    private readonly IRepository<Branch> _branchRepository;

    protected BranchScopedController(IRepository<Branch> branchRepository)
    {
        _branchRepository = branchRepository;
    }

    protected async Task<Guid> GetBranchIdAsync(CancellationToken cancellationToken)
    {
        var candidates = new HashSet<Guid>();

        if (TryReadBranchId(RouteData.Values["branchId"]?.ToString(), out var routeBranchId))
        {
            candidates.Add(routeBranchId);
        }

        if (TryReadBranchId(Request.Query["branchId"].FirstOrDefault(), out var queryBranchId))
        {
            candidates.Add(queryBranchId);
        }

        if (TryReadBranchId(Request.Headers["X-Branch-Id"].FirstOrDefault(), out var headerBranchId))
        {
            candidates.Add(headerBranchId);
        }

        var claimBranchIds = User.Claims
            .Where(c => c.Type is "branch_id" or "branchId" or "branch")
            .Select(c => c.Value);

        foreach (var value in claimBranchIds)
        {
            if (TryReadBranchId(value, out var claimBranchId))
            {
                candidates.Add(claimBranchId);
            }
        }

        foreach (var candidate in candidates)
        {
            var exists = await _branchRepository.Query()
                .AnyAsync(x => x.Id == candidate && x.IsActive, cancellationToken);

            if (exists)
            {
                return candidate;
            }
        }

        var activeBranchIds = await _branchRepository.Query()
            .Where(x => x.IsActive)
            .Select(x => x.Id)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (activeBranchIds.Count == 1)
        {
            return activeBranchIds[0];
        }

        return Guid.Empty;
    }

    protected IActionResult HandleValidationFailure(ValidationException exception, string actionName)
    {
        TempData["Error"] = exception.Message;
        return RedirectToAction(actionName);
    }

    private static bool TryReadBranchId(string? input, out Guid branchId)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            branchId = Guid.Empty;
            return false;
        }

        return Guid.TryParse(input, out branchId) && branchId != Guid.Empty;
    }
}
