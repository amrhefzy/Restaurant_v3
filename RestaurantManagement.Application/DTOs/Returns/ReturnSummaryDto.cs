namespace RestaurantManagement.Application.DTOs.Returns;

public sealed class ReturnSummaryDto
{
    public int SalesReturnsCount { get; set; }
    public decimal SalesReturnsTotal { get; set; }
    public int PurchaseReturnsCount { get; set; }
    public decimal PurchaseReturnsTotal { get; set; }
}
