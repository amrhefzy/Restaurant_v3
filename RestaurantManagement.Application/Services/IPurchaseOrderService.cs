using RestaurantManagement.Application.DTOs.PurchaseOrders;

namespace RestaurantManagement.Application.Services;

public interface IPurchaseOrderService
{
    Task<IReadOnlyCollection<PurchaseOrderListItemDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDetailsDto?> GetByIdAsync(Guid branchId, Guid purchaseOrderId, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDetailsDto> CreateDraftAsync(CreatePurchaseOrderDto request, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDetailsDto> SubmitAsync(SubmitPurchaseOrderRequestDto request, CancellationToken cancellationToken = default);
    Task<PurchaseOrderDetailsDto> ReceiveAsync(ReceivePurchaseOrderRequestDto request, CancellationToken cancellationToken = default);
}
