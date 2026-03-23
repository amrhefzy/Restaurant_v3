using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Orders;

public sealed class CreateOrderDto
{
    public Guid BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public OrderType OrderType { get; set; } = OrderType.Takeaway;
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public IList<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
}
