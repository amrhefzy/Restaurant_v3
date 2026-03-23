using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.PurchaseOrders;

public sealed class PurchaseOrderListItemDto
{
    public Guid Id { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal Total { get; set; }
}
