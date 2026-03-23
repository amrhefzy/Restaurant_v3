using RestaurantManagement.Application.DTOs.Kitchen;

namespace RestaurantManagement.Application.Services;

public interface IKitchenService
{
    Task<IReadOnlyCollection<KitchenOrderCardDto>> GetActiveOrdersAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<bool> StartOrderAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default);
    Task<bool> MarkReadyAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default);
}
