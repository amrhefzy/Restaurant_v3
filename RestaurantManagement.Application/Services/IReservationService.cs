using RestaurantManagement.Application.DTOs.Reservations;

namespace RestaurantManagement.Application.Services;

public interface IReservationService
{
    Task<IReadOnlyCollection<ReservationDto>> GetUpcomingByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<UpdateReservationDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default);
    Task<ReservationDto> CreateAsync(CreateReservationDto request, CancellationToken cancellationToken = default);
    Task<ReservationDto> UpdateAsync(UpdateReservationDto request, CancellationToken cancellationToken = default);
}
