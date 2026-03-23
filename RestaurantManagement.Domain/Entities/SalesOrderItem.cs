using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public class SalesOrderItem : BaseEntity
{
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Note { get; set; }

    public SalesOrder? SalesOrder { get; set; }
    public Product? Product { get; set; }
}
