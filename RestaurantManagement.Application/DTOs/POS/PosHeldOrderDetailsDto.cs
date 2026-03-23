using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.POS;

public sealed class PosHeldOrderDetailsDto
{
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? HeldReference { get; set; }
    public OrderType OrderType { get; set; }
    public Guid? TableId { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyCollection<PosHeldOrderItemDto> Items { get; set; } = Array.Empty<PosHeldOrderItemDto>();
}

public sealed class PosHeldOrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Note { get; set; }
}
