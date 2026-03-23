using RestaurantManagement.Application.DTOs.PurchaseOrders;

namespace RestaurantManagement.Application.Services;

public interface IPurchaseOrderService
{
    Task<IReadOnlyCollection<PurchaseOrderListItemDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default);
}
