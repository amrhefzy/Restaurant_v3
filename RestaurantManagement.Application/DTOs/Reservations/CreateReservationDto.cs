namespace RestaurantManagement.Application.DTOs.Reservations;

public sealed class CreateReservationDto
{
    public Guid BranchId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public DateTime ReservationAtUtc { get; set; }
    public int PartySize { get; set; }
    public string? Notes { get; set; }
}
