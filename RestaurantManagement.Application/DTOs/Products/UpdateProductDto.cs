namespace RestaurantManagement.Application.DTOs.Products;

public sealed class UpdateProductDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid CategoryId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsStockTracked { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
}
