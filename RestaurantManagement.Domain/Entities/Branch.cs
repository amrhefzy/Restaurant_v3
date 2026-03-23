using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class Branch : SoftDeletableAuditableEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }

    public ICollection<RestaurantTable> Tables { get; set; } = new HashSet<RestaurantTable>();
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    public ICollection<SalesOrder> SalesOrders { get; set; } = new HashSet<SalesOrder>();
    public ICollection<CashierShift> CashierShifts { get; set; } = new HashSet<CashierShift>();
    public ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new HashSet<PaymentTransaction>();
}
