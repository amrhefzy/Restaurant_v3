using RestaurantManagement.Application.DTOs.Inventory;

namespace RestaurantManagement.Application.Services;

public interface IInventoryService
{
    Task<IReadOnlyCollection<InventoryMovementDto>> GetRecentMovementsAsync(Guid branchId, int take = 100, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<LowStockItemDto>> GetLowStockItemsAsync(Guid branchId, CancellationToken cancellationToken = default);
}
