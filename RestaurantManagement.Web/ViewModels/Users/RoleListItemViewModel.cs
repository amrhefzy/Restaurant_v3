namespace RestaurantManagement.Web.ViewModels.Users;

public sealed class RoleListItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UsersCount { get; set; }
}
