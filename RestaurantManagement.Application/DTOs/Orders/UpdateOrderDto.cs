using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Orders;

public sealed class UpdateOrderDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public OrderType OrderType { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public IList<UpdateOrderItemDto> Items { get; set; } = new List<UpdateOrderItemDto>();
}
