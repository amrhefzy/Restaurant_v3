using RestaurantManagement.Application.DTOs.POS;

namespace RestaurantManagement.Application.Services;

public interface IPosService
{
    Task<PosScreenDataDto> GetScreenDataAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<PosCheckoutResponseDto> ProcessOrderAsync(PosCheckoutRequestDto request, CancellationToken cancellationToken = default);
    Task<PosHeldOrderDetailsDto?> GetHeldOrderAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PosOrderStatusDto>> GetActiveOrderStatusesAsync(Guid branchId, CancellationToken cancellationToken = default);
}
