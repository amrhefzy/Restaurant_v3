using RestaurantManagement.Application.DTOs.Inventory;

namespace RestaurantManagement.Web.ViewModels.Inventory;

public sealed class InventoryOverviewViewModel
{
    public IReadOnlyCollection<LowStockItemDto> LowStockItems { get; set; } = Array.Empty<LowStockItemDto>();
    public IReadOnlyCollection<InventoryMovementDto> RecentMovements { get; set; } = Array.Empty<InventoryMovementDto>();
}
