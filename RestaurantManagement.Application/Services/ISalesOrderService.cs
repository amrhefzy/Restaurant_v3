using RestaurantManagement.Application.DTOs.SalesOrders;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.Services;

public interface ISalesOrderService
{
    Task<IReadOnlyCollection<SalesOrderListItemDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default);
    Task MarkAsPaidAsync(Guid branchId, Guid salesOrderId, PaymentType paymentType, CancellationToken cancellationToken = default);
    Task CloseAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default);
}
