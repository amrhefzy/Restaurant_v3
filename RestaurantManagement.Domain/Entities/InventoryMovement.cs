using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public sealed class InventoryMovement : AuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid ProductId { get; set; }
    public InventoryMovementType MovementType { get; set; }
    public decimal QuantityChange { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Reason { get; set; }

    public Branch? Branch { get; set; }
    public Product? Product { get; set; }
}
