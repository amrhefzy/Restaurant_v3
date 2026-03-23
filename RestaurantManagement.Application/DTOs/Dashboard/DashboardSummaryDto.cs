namespace RestaurantManagement.Application.DTOs.Dashboard;

public sealed class DashboardSummaryDto
{
    public decimal TodaySales { get; set; }
    public decimal WeeklySales { get; set; }
    public decimal AverageTicket { get; set; }
    public int TotalOrdersToday { get; set; }
    public int ActiveReservations { get; set; }
    public int LowStockItems { get; set; }
    public IReadOnlyCollection<TopProductDto> TopProducts { get; set; } = Array.Empty<TopProductDto>();
    public IReadOnlyCollection<CashierPerformanceDto> CashierPerformance { get; set; } = Array.Empty<CashierPerformanceDto>();
    public IReadOnlyCollection<DashboardRecentOrderDto> RecentOrders { get; set; } = Array.Empty<DashboardRecentOrderDto>();
    public IReadOnlyCollection<DashboardRecentReservationDto> RecentReservations { get; set; } = Array.Empty<DashboardRecentReservationDto>();
}

public sealed class TopProductDto
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
}

public sealed class CashierPerformanceDto
{
    public string CashierName { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
}

public sealed class DashboardRecentOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public sealed class DashboardRecentReservationDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public DateTime ReservationAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
}
