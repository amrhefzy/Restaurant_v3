using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.DTOs.Dashboard;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid branchId, DateTime businessDateUtc, CancellationToken cancellationToken = default)
    {
        var dayStart = businessDateUtc.Date;
        var dayEnd = dayStart.AddDays(1);

        var todayPaidOrders = _dbContext.SalesOrders
            .Where(x => x.BranchId == branchId
                        && x.CreatedOn >= dayStart
                        && x.CreatedOn < dayEnd
                        && x.Status != OrderStatus.Cancelled);

        var todaySales = await todayPaidOrders.SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;
        var totalOrdersToday = await todayPaidOrders.CountAsync(cancellationToken);
        var averageTicket = totalOrdersToday > 0
            ? decimal.Round(todaySales / totalOrdersToday, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var weekStart = dayStart.AddDays(-6);
        var weeklySales = await _dbContext.SalesOrders
            .Where(x => x.BranchId == branchId
                        && x.CreatedOn >= weekStart
                        && x.CreatedOn < dayEnd
                        && x.Status != OrderStatus.Cancelled)
            .SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;

        var activeReservations = await _dbContext.Reservations
            .CountAsync(x => x.BranchId == branchId
                             && x.ReservationAtUtc >= dayStart
                             && x.ReservationAtUtc < dayEnd
                             && x.Status != ReservationStatus.Cancelled
                             && x.Status != ReservationStatus.Completed, cancellationToken);

        var lowStockItems = await _dbContext.Products
            .Where(x => x.BranchId == branchId && x.IsStockTracked)
            .CountAsync(x => _dbContext.InventoryMovements
                .Where(m => m.ProductId == x.Id)
                .Sum(m => m.QuantityChange) <= x.ReorderLevel, cancellationToken);

        var topProducts = await _dbContext.SalesOrderItems
            .Where(x => x.SalesOrder != null
                        && x.SalesOrder.BranchId == branchId
                        && x.SalesOrder.CreatedOn >= dayStart
                        && x.SalesOrder.CreatedOn < dayEnd)
            .GroupBy(x => x.ProductName)
            .Select(g => new TopProductDto
            {
                ProductName = g.Key,
                QuantitySold = (int)g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentOrders = await _dbContext.SalesOrders
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(6)
            .Select(x => new DashboardRecentOrderDto
            {
                OrderNumber = x.OrderNumber,
                OrderType = x.OrderType.ToString(),
                Status = x.Status.ToString(),
                Total = x.Total
            })
            .ToListAsync(cancellationToken);

        var recentReservations = await _dbContext.Reservations
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.ReservationAtUtc)
            .Take(6)
            .Select(x => new DashboardRecentReservationDto
            {
                CustomerName = x.Customer != null ? x.Customer.Name : string.Empty,
                TableNumber = x.Table != null ? x.Table.TableNumber : "-",
                ReservationAtUtc = x.ReservationAtUtc,
                Status = x.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TodaySales = todaySales,
            WeeklySales = weeklySales,
            AverageTicket = averageTicket,
            TotalOrdersToday = totalOrdersToday,
            ActiveReservations = activeReservations,
            LowStockItems = lowStockItems,
            TopProducts = topProducts,
            RecentOrders = recentOrders,
            RecentReservations = recentReservations,
            CashierPerformance = new[]
            {
                new CashierPerformanceDto { CashierName = "Cashier A", SalesAmount = 0m },
                new CashierPerformanceDto { CashierName = "Cashier B", SalesAmount = 0m }
            }
        };
    }
}
