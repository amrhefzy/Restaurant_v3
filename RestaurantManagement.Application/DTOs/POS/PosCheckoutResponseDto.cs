namespace RestaurantManagement.Application.DTOs.POS;

public sealed class PosCheckoutResponseDto
{
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? HeldReference { get; set; }
}
