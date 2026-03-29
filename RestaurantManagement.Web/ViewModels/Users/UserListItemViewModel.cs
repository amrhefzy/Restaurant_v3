namespace RestaurantManagement.Web.ViewModels.Users;

public sealed class UserListItemViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid? DefaultBranchId { get; set; }
}
