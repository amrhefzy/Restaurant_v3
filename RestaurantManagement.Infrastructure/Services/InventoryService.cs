using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.DTOs.Inventory;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly AppDbContext _dbContext;

    public InventoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<InventoryMovementDto>> GetRecentMovementsAsync(Guid branchId, int take = 100, CancellationToken cancellationToken = default)
    {
        return await _dbContext.InventoryMovements
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(take)
            .Select(x => new InventoryMovementDto
            {
                CreatedOn = x.CreatedOn,
                ProductName = x.Product != null ? x.Product.NameEn : string.Empty,
                MovementType = x.MovementType,
                QuantityChange = x.QuantityChange,
                ReferenceNumber = x.ReferenceNumber
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<LowStockItemDto>> GetLowStockItemsAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsStockTracked)
            .Select(x => new LowStockItemDto
            {
                ProductId = x.Id,
                ProductName = x.NameEn,
                ReorderLevel = x.ReorderLevel,
                OnHandQuantity = _dbContext.InventoryMovements
                    .Where(m => m.ProductId == x.Id)
                    .Select(m => m.QuantityChange)
                    .DefaultIfEmpty(0)
                    .Sum()
            })
            .Where(x => x.OnHandQuantity <= x.ReorderLevel)
            .OrderBy(x => x.OnHandQuantity)
            .ToListAsync(cancellationToken);
    }
}
