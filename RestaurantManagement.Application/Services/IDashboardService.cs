using RestaurantManagement.Application.DTOs.Dashboard;

namespace RestaurantManagement.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(Guid branchId, DateTime businessDateUtc, CancellationToken cancellationToken = default);
}
