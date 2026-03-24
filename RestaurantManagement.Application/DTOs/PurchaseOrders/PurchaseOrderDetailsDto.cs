using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.PurchaseOrders;

public sealed class PurchaseOrderDetailsDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? SubmittedOnUtc { get; set; }
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public IReadOnlyCollection<PurchaseOrderItemDetailsDto> Items { get; set; } = Array.Empty<PurchaseOrderItemDetailsDto>();
}

public sealed class PurchaseOrderItemDetailsDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}
