using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.DTOs.Kitchen;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class KitchenService : IKitchenService
{
    private readonly AppDbContext _dbContext;

    public KitchenService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<KitchenOrderCardDto>> GetActiveOrdersAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var orders = await _dbContext.SalesOrders
            .AsNoTracking()
            .Where(x => x.BranchId == branchId
                        && (x.Status == OrderStatus.New
                            || x.Status == OrderStatus.InKitchen
                            || x.Status == OrderStatus.Ready))
            .OrderBy(x => x.CreatedOn)
            .Select(x => new KitchenOrderCardDto
            {
                SalesOrderId = x.Id,
                OrderNumber = x.OrderNumber,
                Status = x.Status.ToString(),
                CreatedOn = x.CreatedOn,
                MinutesSinceCreated = EF.Functions.DateDiffMinute(x.CreatedOn, now),
                Notes = x.Notes,
                Items = x.Items
                    .OrderBy(i => i.ProductName)
                    .Select(i => new KitchenOrderItemDto
                    {
                        ProductName = i.ProductName,
                        Quantity = i.Quantity,
                        Note = i.Note
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return orders;
    }

    public async Task<bool> StartOrderAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.SalesOrders
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Id == salesOrderId, cancellationToken);

        if (order is null || order.Status != OrderStatus.New)
        {
            return false;
        }

        order.Status = OrderStatus.InKitchen;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkReadyAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.SalesOrders
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Id == salesOrderId, cancellationToken);

        if (order is null || (order.Status != OrderStatus.InKitchen && order.Status != OrderStatus.New))
        {
            return false;
        }

        order.Status = OrderStatus.Ready;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CompleteOrderAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _dbContext.SalesOrders
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Id == salesOrderId, cancellationToken);

        if (order is null || order.Status != OrderStatus.Ready)
        {
            return false;
        }

        order.Status = OrderStatus.Served;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
