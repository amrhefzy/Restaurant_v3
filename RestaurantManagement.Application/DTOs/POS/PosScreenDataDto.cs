using RestaurantManagement.Application.DTOs.Categories;
using RestaurantManagement.Application.DTOs.Tables;

namespace RestaurantManagement.Application.DTOs.POS;

public sealed class PosScreenDataDto
{
    public IReadOnlyCollection<CategoryDto> Categories { get; set; } = Array.Empty<CategoryDto>();
    public IReadOnlyCollection<PosProductDto> Products { get; set; } = Array.Empty<PosProductDto>();
    public IReadOnlyCollection<TableDto> Tables { get; set; } = Array.Empty<TableDto>();
    public IReadOnlyCollection<PosHeldOrderDto> HeldOrders { get; set; } = Array.Empty<PosHeldOrderDto>();
    public IReadOnlyCollection<PosOrderStatusDto> ActiveOrders { get; set; } = Array.Empty<PosOrderStatusDto>();
}

public sealed class PosProductDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public bool IsActive { get; set; }
}

public sealed class PosHeldOrderDto
{
    public Guid SalesOrderId { get; set; }
    public string HeldReference { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedOn { get; set; }
}

public sealed class PosOrderStatusDto
{
    public Guid SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public bool IsReady { get; set; }
}
