namespace RestaurantManagement.Application.DTOs.Reports;

public sealed class ReportOverviewDto
{
    public decimal GrossSales { get; set; }
    public int OrdersCount { get; set; }
    public decimal AverageTicket { get; set; }
    public int LowStockCount { get; set; }
    public IReadOnlyCollection<SalesTrendPointDto> SalesTrend { get; set; } = Array.Empty<SalesTrendPointDto>();
    public IReadOnlyCollection<TopCategorySalesDto> TopCategories { get; set; } = Array.Empty<TopCategorySalesDto>();
}

public sealed class SalesTrendPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
}

public sealed class TopCategorySalesDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
}
