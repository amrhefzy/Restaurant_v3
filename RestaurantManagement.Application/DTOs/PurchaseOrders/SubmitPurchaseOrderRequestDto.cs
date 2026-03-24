namespace RestaurantManagement.Application.DTOs.PurchaseOrders;

public sealed class SubmitPurchaseOrderRequestDto
{
    public Guid BranchId { get; set; }
    public Guid PurchaseOrderId { get; set; }
}
