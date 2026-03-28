using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.DTOs.Reports;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReportOverviewDto> GetOverviewAsync(Guid branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var ordersQuery = _dbContext.SalesOrders
            .Where(x => x.BranchId == branchId
                        && x.CreatedOn >= fromUtc
                        && x.CreatedOn <= toUtc
                        && x.Status != OrderStatus.Cancelled);

        var grossSales = await ordersQuery.SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;
        var ordersCount = await ordersQuery.CountAsync(cancellationToken);

        var lowStockCount = await _dbContext.Products
            .Where(x => x.BranchId == branchId && x.IsStockTracked)
            .GroupJoin(
                _dbContext.InventoryMovements,
                product => product.Id,
                movement => movement.ProductId,
                (product, movements) => new
                {
                    product.ReorderLevel,
                    OnHand = movements.Sum(m => (decimal?)m.QuantityChange) ?? 0m
                })
            .CountAsync(x => x.OnHand <= x.ReorderLevel, cancellationToken);

        var trend = await _dbContext.SalesOrders
            .Where(x => x.BranchId == branchId
                        && x.CreatedOn >= fromUtc
                        && x.CreatedOn <= toUtc
                        && x.Status != OrderStatus.Cancelled)
            .GroupBy(x => x.CreatedOn.Date)
            .Select(g => new SalesTrendPointDto
            {
                Label = g.Key.ToString("MM-dd"),
                SalesAmount = g.Sum(x => x.Total)
            })
            .OrderBy(x => x.Label)
            .ToListAsync(cancellationToken);

        var topCategories = await _dbContext.SalesOrderItems
            .Where(x => x.SalesOrder != null
                        && x.SalesOrder.BranchId == branchId
                        && x.SalesOrder.CreatedOn >= fromUtc
                        && x.SalesOrder.CreatedOn <= toUtc)
            .Join(_dbContext.Products,
                item => item.ProductId,
                product => product.Id,
                (item, product) => new { item.LineTotal, product.CategoryId })
            .Join(_dbContext.Categories,
                x => x.CategoryId,
                c => c.Id,
                (x, c) => new { x.LineTotal, CategoryName = c.NameEn })
            .GroupBy(x => x.CategoryName)
            .Select(g => new TopCategorySalesDto
            {
                CategoryName = g.Key,
                SalesAmount = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.SalesAmount)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new ReportOverviewDto
        {
            GrossSales = grossSales,
            OrdersCount = ordersCount,
            AverageTicket = ordersCount > 0 ? decimal.Round(grossSales / ordersCount, 2, MidpointRounding.AwayFromZero) : 0m,
            LowStockCount = lowStockCount,
            SalesTrend = trend,
            TopCategories = topCategories
        };
    }
}
