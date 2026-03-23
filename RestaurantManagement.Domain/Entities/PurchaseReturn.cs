using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class PurchaseReturn : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string? Reason { get; set; }

    public Branch? Branch { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public ICollection<PurchaseReturnItem> Items { get; set; } = new HashSet<PurchaseReturnItem>();
}
