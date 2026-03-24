namespace RestaurantManagement.Application.DTOs.PurchaseOrders;

public sealed class ReceivePurchaseOrderRequestDto
{
    public Guid BranchId { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public string? ReceiptReferenceNumber { get; set; }
    public DateTime? ReceivedOnUtc { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyCollection<ReceivePurchaseOrderLineDto> Items { get; set; } = Array.Empty<ReceivePurchaseOrderLineDto>();
}

public sealed class ReceivePurchaseOrderLineDto
{
    public Guid PurchaseOrderItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
}
