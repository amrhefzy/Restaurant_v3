using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Inventory;

public sealed class InventoryMovementHistoryItemDto
{
    public Guid MovementId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public InventoryMovementType MovementType { get; set; }
    public decimal QuantityChange { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime CreatedOn { get; set; }
}
