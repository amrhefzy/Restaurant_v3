namespace RestaurantManagement.Application.DTOs.Kitchen;

public sealed class KitchenOrderCardDto
{
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public int MinutesSinceCreated { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyCollection<KitchenOrderItemDto> Items { get; set; } = Array.Empty<KitchenOrderItemDto>();
}

public sealed class KitchenOrderItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Note { get; set; }
}
