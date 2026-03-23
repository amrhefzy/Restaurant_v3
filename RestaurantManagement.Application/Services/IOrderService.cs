using RestaurantManagement.Application.DTOs.Orders;

namespace RestaurantManagement.Application.Services;

public interface IOrderService
{
    Task<IReadOnlyCollection<OrderDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default);
    Task<UpdateOrderDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderDto request, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateAsync(UpdateOrderDto request, CancellationToken cancellationToken = default);
}
