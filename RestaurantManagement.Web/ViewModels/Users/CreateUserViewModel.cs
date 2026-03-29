using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RestaurantManagement.Web.ViewModels.Users;

public sealed class CreateUserViewModel
{
    public Guid? DefaultBranchId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    [Required]
    public string RoleName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public IReadOnlyCollection<SelectListItem> RoleOptions { get; set; } = Array.Empty<SelectListItem>();
}
