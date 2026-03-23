using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public sealed class CashierShift : AuditableEntity
{
    public Guid BranchId { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    public decimal OpeningCash { get; set; }
    public DateTime OpenedOnUtc { get; set; } = DateTime.UtcNow;
    public string? OpenedByUserId { get; set; }
    public string? OpenedByUserName { get; set; }
    public DateTime? ClosedOnUtc { get; set; }
    public string? ClosedByUserId { get; set; }
    public string? ClosedByUserName { get; set; }
    public decimal? ClosingCashActual { get; set; }
    public decimal? ClosingCashExpected { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalCard { get; set; }
    public decimal CashVariance { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new HashSet<PaymentTransaction>();
}
