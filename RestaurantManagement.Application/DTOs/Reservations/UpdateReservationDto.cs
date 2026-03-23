using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Reservations;

public sealed class UpdateReservationDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public DateTime ReservationAtUtc { get; set; }
    public int PartySize { get; set; }
    public ReservationStatus Status { get; set; }
    public string? Notes { get; set; }
}
