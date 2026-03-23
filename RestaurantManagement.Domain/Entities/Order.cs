using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public sealed class Order : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType OrderType { get; set; } = OrderType.Takeaway;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }

    public Branch? Branch { get; set; }
    public Customer? Customer { get; set; }
    public RestaurantTable? Table { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();
}
