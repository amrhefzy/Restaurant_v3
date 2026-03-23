using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.POS;

public sealed class PosCheckoutRequestDto
{
    public Guid BranchId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? TableId { get; set; }
    public OrderType OrderType { get; set; } = OrderType.Takeaway;
    public PaymentType? PaymentType { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Notes { get; set; }
    public string? HeldReference { get; set; }
    public bool IsHold { get; set; }
    public IReadOnlyCollection<PosCheckoutItemDto> Items { get; set; } = Array.Empty<PosCheckoutItemDto>();
}

public sealed class PosCheckoutItemDto
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public string? Note { get; set; }
}
