using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Inventory;

public sealed class InventoryMovementHistoryRequestDto
{
    public Guid BranchId { get; set; }
    public Guid? ProductId { get; set; }
    public InventoryMovementType? MovementType { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 100;
}
