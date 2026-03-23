using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public sealed class Reservation : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public DateTime ReservationAtUtc { get; set; }
    public int PartySize { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public string? Notes { get; set; }

    public Branch? Branch { get; set; }
    public Customer? Customer { get; set; }
    public RestaurantTable? Table { get; set; }
}
