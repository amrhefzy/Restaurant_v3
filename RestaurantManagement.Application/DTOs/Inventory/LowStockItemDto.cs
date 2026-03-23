namespace RestaurantManagement.Application.DTOs.Inventory;

public sealed class LowStockItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal OnHandQuantity { get; set; }
    public decimal ReorderLevel { get; set; }
}
