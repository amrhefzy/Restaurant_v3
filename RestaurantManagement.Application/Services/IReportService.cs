using RestaurantManagement.Application.DTOs.Reports;

namespace RestaurantManagement.Application.Services;

public interface IReportService
{
    Task<ReportOverviewDto> GetOverviewAsync(Guid branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
