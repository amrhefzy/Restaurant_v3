namespace RestaurantManagement.Application.DTOs.PurchaseOrders;

public sealed class CreatePurchaseOrderDto
{
    public Guid BranchId { get; set; }
    public Guid SupplierId { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyCollection<CreatePurchaseOrderItemDto> Items { get; set; } = Array.Empty<CreatePurchaseOrderItemDto>();
}

public sealed class CreatePurchaseOrderItemDto
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
