using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class PurchaseReturnItem : BaseEntity
{
    public Guid PurchaseReturnId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }

    public PurchaseReturn? PurchaseReturn { get; set; }
    public Product? Product { get; set; }
}
