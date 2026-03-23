using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Reservations;

public sealed class ReservationDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? TableNumber { get; set; }
    public DateTime ReservationAtUtc { get; set; }
    public int PartySize { get; set; }
    public ReservationStatus Status { get; set; }
}
