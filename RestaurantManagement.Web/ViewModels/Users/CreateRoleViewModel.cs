using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Web.ViewModels.Users;

public sealed class CreateRoleViewModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
