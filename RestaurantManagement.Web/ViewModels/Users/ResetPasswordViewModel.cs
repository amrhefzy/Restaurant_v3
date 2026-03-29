using System.ComponentModel.DataAnnotations;

namespace RestaurantManagement.Web.ViewModels.Users;

public sealed class ResetPasswordViewModel
{
    [Required]
    public Guid UserId { get; set; }

    public string UserDisplayName { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;
}
