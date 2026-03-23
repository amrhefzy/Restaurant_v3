using RestaurantManagement.Application.DTOs.Returns;

namespace RestaurantManagement.Application.Services;

public interface IReturnService
{
    Task<ReturnSummaryDto> GetSummaryAsync(Guid branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
