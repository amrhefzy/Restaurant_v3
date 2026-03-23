namespace RestaurantManagement.Application.DTOs.Orders;

public sealed class UpdateOrderItemDto
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Note { get; set; }
}
