using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public sealed class PaymentTransaction : AuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid SalesOrderId { get; set; }
    public Guid CashierShiftId { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidOnUtc { get; set; } = DateTime.UtcNow;
    public string? ReferenceNumber { get; set; }

    public Branch? Branch { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public CashierShift? CashierShift { get; set; }
}
