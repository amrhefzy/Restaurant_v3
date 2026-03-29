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
                IsActive = user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow,
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
            LockoutEnabled = true,
            LockoutEnd = model.IsActive ? null : DateTimeOffset.MaxValue
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

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            TempData["Error"] = _localizer["UserWasNotFound"].Value;
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var model = new EditUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            RoleName = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow,
            DefaultBranchId = user.DefaultBranchId
        };

        await LoadRoleOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditUserViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;
        await LoadRoleOptionsAsync(model, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            TempData["Error"] = _localizer["UserWasNotFound"].Value;
            return RedirectToAction(nameof(Index));
        }

        var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
        if (existingByEmail is not null && existingByEmail.Id != id)
        {
            ModelState.AddModelError(nameof(model.Email), _localizer["EmailAlreadyExists"].Value);
            return View(model);
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.DefaultBranchId = model.DefaultBranchId;
        user.LockoutEnabled = true;
        user.LockoutEnd = model.IsActive ? null : DateTimeOffset.MaxValue;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeRoles = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeRoles.Succeeded)
            {
                foreach (var error in removeRoles.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        var addRole = await _userManager.AddToRoleAsync(user, model.RoleName);
        if (!addRole.Succeeded)
        {
            foreach (var error in addRole.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = _localizer["UserUpdatedSuccessfully"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            TempData["Error"] = _localizer["UserWasNotFound"].Value;
            return RedirectToAction(nameof(Index));
        }

        var isCurrentlyActive = user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow;
        user.LockoutEnabled = true;
        user.LockoutEnd = isCurrentlyActive ? DateTimeOffset.MaxValue : null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join("; ", result.Errors.Select(x => x.Description));
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = isCurrentlyActive
            ? _localizer["UserDeactivatedSuccessfully"].Value
            : _localizer["UserActivatedSuccessfully"].Value;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            TempData["Error"] = _localizer["UserWasNotFound"].Value;
            return RedirectToAction(nameof(Index));
        }

        return View(new ResetPasswordViewModel
        {
            UserId = user.Id,
            UserDisplayName = string.IsNullOrWhiteSpace(user.FullName) ? (user.Email ?? string.Empty) : user.FullName
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Id == model.UserId, cancellationToken);
        if (user is null)
        {
            TempData["Error"] = _localizer["UserWasNotFound"].Value;
            return RedirectToAction(nameof(Index));
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = _localizer["PasswordResetSuccessfully"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Roles(CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var model = new List<RoleListItemViewModel>(roles.Count);
        foreach (var role in roles)
        {
            var userCount = await _userManager.GetUsersInRoleAsync(role.Name!);
            model.Add(new RoleListItemViewModel
            {
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                UsersCount = userCount.Count
            });
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult CreateRole()
    {
        return View(new CreateRoleViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var exists = await _roleManager.RoleExistsAsync(model.Name);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), _localizer["RoleAlreadyExists"].Value);
            return View(model);
        }

        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            NormalizedName = model.Name.ToUpperInvariant(),
            Description = model.Description
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = _localizer["RoleCreatedSuccessfully"].Value;
        return RedirectToAction(nameof(Roles));
    }

    [HttpGet]
    public async Task<IActionResult> EditRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            TempData["Error"] = _localizer["RoleWasNotFound"].Value;
            return RedirectToAction(nameof(Roles));
        }

        return View(new EditRoleViewModel
        {
            Name = role.Name ?? string.Empty,
            Description = role.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(string id, EditRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var role = await _roleManager.FindByIdAsync(id);
        if (role is null)
        {
            TempData["Error"] = _localizer["RoleWasNotFound"].Value;
            return RedirectToAction(nameof(Roles));
        }

        var existing = await _roleManager.FindByNameAsync(model.Name);
        if (existing is not null && existing.Id != role.Id)
        {
            ModelState.AddModelError(nameof(model.Name), _localizer["RoleAlreadyExists"].Value);
            return View(model);
        }

        role.Name = model.Name;
        role.NormalizedName = model.Name.ToUpperInvariant();
        role.Description = model.Description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        TempData["Success"] = _localizer["RoleUpdatedSuccessfully"].Value;
        return RedirectToAction(nameof(Roles));
    }

    private async Task LoadRoleOptionsAsync(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name!, x.Name!))
            .ToListAsync(cancellationToken);

        model.RoleOptions = roles;
    }

    private async Task LoadRoleOptionsAsync(EditUserViewModel model, CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name!, x.Name!))
            .ToListAsync(cancellationToken);

        model.RoleOptions = roles;
    }
}
