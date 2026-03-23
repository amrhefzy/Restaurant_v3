namespace RestaurantManagement.Application.DTOs.Suppliers;

public sealed class SupplierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}
