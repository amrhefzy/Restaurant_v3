namespace RestaurantManagement.Application.DTOs.Customers;

public sealed class CustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsWalkIn { get; set; }
    public bool IsActive { get; set; }
}
