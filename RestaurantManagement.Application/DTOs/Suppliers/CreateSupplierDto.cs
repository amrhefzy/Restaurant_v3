namespace RestaurantManagement.Application.DTOs.Suppliers;

public sealed class CreateSupplierDto
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
