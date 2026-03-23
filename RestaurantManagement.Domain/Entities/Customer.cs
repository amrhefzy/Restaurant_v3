using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class Customer : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsWalkIn { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    public ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
}
