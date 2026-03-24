namespace RestaurantManagement.Application.DTOs.Inventory;

public sealed class StockOnHandItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal ReorderLevel { get; set; }
    public decimal OnHandQuantity { get; set; }
}
