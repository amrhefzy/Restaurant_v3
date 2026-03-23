using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public sealed class RestaurantTable : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public TableStatus Status { get; set; } = TableStatus.Free;

    public Branch? Branch { get; set; }
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    public ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
}
