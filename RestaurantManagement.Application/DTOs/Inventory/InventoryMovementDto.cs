using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Inventory;

public sealed class InventoryMovementDto
{
    public DateTime CreatedOn { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public InventoryMovementType MovementType { get; set; }
    public decimal QuantityChange { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
