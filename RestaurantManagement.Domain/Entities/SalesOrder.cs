using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Domain.Entities;

public class SalesOrder : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentType? PaymentType { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaidOnUtc { get; set; }
    public Guid? CashierShiftId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public string? HeldReference { get; set; }

    public Branch? Branch { get; set; }
    public Customer? Customer { get; set; }
    public RestaurantTable? Table { get; set; }
    public CashierShift? CashierShift { get; set; }
    public ICollection<SalesOrderItem> Items { get; set; } = new HashSet<SalesOrderItem>();
    public ICollection<SalesReturn> SalesReturns { get; set; } = new HashSet<SalesReturn>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new HashSet<PaymentTransaction>();
}
