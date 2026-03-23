using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.SalesOrders;

public sealed class SalesOrderListItemDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public PaymentType? PaymentType { get; set; }
    public decimal Total { get; set; }
}
