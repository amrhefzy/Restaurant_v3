using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public sealed class PurchaseOrder : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid SupplierId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public DateTime? SubmittedOnUtc { get; set; }
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }

    public Branch? Branch { get; set; }
    public Supplier? Supplier { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new HashSet<PurchaseOrderItem>();
    public ICollection<PurchaseReturn> PurchaseReturns { get; set; } = new HashSet<PurchaseReturn>();
}
