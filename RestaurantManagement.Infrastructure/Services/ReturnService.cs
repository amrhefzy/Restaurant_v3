using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.DTOs.Returns;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class ReturnService : IReturnService
{
    private readonly AppDbContext _dbContext;

    public ReturnService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReturnSummaryDto> GetSummaryAsync(Guid branchId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var salesReturnsQuery = _dbContext.SalesReturns.Where(x => x.BranchId == branchId && x.CreatedOn >= fromUtc && x.CreatedOn <= toUtc);
        var purchaseReturnsQuery = _dbContext.PurchaseReturns.Where(x => x.BranchId == branchId && x.CreatedOn >= fromUtc && x.CreatedOn <= toUtc);

        return new ReturnSummaryDto
        {
            SalesReturnsCount = await salesReturnsQuery.CountAsync(cancellationToken),
            SalesReturnsTotal = await salesReturnsQuery.SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m,
            PurchaseReturnsCount = await purchaseReturnsQuery.CountAsync(cancellationToken),
            PurchaseReturnsTotal = await purchaseReturnsQuery.SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m
        };
    }
}
