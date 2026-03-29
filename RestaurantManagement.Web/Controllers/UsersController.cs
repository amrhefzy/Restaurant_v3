using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Identity;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.ViewModels.Users;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class UsersController : BranchScopedController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IStringLocalizer<SharedResource> localizer,
        IRepository<Branch> branchRepository) : base(branchRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .ToListAsync(cancellationToken);

        var model = new List<UserListItemViewModel>(users.Count);
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                RoleName = roles.FirstOrDefault() ?? _localizer["NoRoleAssigned"].Value,
                IsActive = !user.LockoutEnabled || user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                DefaultBranchId = user.DefaultBranchId
            });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateUserViewModel
        {
            DefaultBranchId = await GetBranchIdAsync(cancellationToken)
        };

        await LoadRoleOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        await LoadRoleOptionsAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var emailExists = await _userManager.FindByEmailAsync(model.Email);
        if (emailExists is not null)
        {
            ModelState.AddModelError(nameof(model.Email), _localizer["EmailAlreadyExists"].Value);
            return View(model);
        }

        var branchId = await GetBranchIdAsync(cancellationToken);
        if (branchId != Guid.Empty)
        {
            model.DefaultBranchId ??= branchId;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            DefaultBranchId = model.DefaultBranchId,
            EmailConfirmed = true,
            LockoutEnabled = !model.IsActive
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.RoleName);
        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = _localizer["UserCreatedSuccessfully"].Value;
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRoleOptionsAsync(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name!, x.Name!))
            .ToListAsync(cancellationToken);

        model.RoleOptions = roles;
    }
}
