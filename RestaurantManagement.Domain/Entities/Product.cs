using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class Product : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid CategoryId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsStockTracked { get; set; } = true;
    public decimal ReorderLevel { get; set; }
    public string? ImageUrl { get; set; }

    public Branch? Branch { get; set; }
    public Category? Category { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new HashSet<SalesOrderItem>();
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new HashSet<PurchaseOrderItem>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new HashSet<InventoryMovement>();
}
