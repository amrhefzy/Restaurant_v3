using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class SalesReturnItem : BaseEntity
{
    public Guid SalesReturnId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public SalesReturn? SalesReturn { get; set; }
    public Product? Product { get; set; }
}
