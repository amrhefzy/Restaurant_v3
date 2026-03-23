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
        return await _branchRepository.Query().Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);
    }

    protected IActionResult HandleValidationFailure(ValidationException exception, string actionName)
    {
        TempData["Error"] = exception.Message;
        return RedirectToAction(actionName);
    }
}
