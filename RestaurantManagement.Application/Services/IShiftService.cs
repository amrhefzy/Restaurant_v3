using RestaurantManagement.Application.DTOs.Shifts;

namespace RestaurantManagement.Application.Services;

public interface IShiftService
{
    Task<ShiftSummaryDto?> GetCurrentAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<ShiftSummaryDto> OpenAsync(OpenShiftRequestDto request, CancellationToken cancellationToken = default);
    Task<ShiftSummaryDto> CloseAsync(CloseShiftRequestDto request, CancellationToken cancellationToken = default);
}
