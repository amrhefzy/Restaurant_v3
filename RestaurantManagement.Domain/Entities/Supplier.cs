using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class Supplier : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    public Branch? Branch { get; set; }
   // public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new HashSet<PurchaseOrder>();
}
