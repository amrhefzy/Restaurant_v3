namespace RestaurantManagement.Application.DTOs.Customers;

public sealed class CreateCustomerDto
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsWalkIn { get; set; }
}
