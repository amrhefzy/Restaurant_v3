using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
    public string? Note { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
