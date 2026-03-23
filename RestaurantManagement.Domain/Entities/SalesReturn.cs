using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class SalesReturn : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid SalesOrderId { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string? Reason { get; set; }

    public Branch? Branch { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public ICollection<SalesReturnItem> Items { get; set; } = new HashSet<SalesReturnItem>();
}
