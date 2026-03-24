using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Inventory;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly IRepository<InventoryMovement> _movementRepository;
    private readonly IRepository<Product> _productRepository;

    public InventoryService(IRepository<InventoryMovement> movementRepository, IRepository<Product> productRepository)
    {
        _movementRepository = movementRepository;
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyCollection<InventoryMovementDto>> GetRecentMovementsAsync(Guid branchId, int take = 100, CancellationToken cancellationToken = default)
    {
        return await _movementRepository.Query()
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(Math.Clamp(take, 1, 500))
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
        return await _productRepository.Query()
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsStockTracked)
            .Select(x => new LowStockItemDto
            {
                ProductId = x.Id,
                ProductName = x.NameEn,
                ReorderLevel = x.ReorderLevel,
                OnHandQuantity = _movementRepository.Query()
                    .Where(m => m.ProductId == x.Id)
                    .Select(m => m.QuantityChange)
                    .DefaultIfEmpty(0)
                    .Sum()
            })
            .Where(x => x.OnHandQuantity <= x.ReorderLevel)
            .OrderBy(x => x.OnHandQuantity)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<InventoryMovementHistoryItemDto>> GetMovementHistoryAsync(InventoryMovementHistoryRequestDto request, CancellationToken cancellationToken = default)
    {
        var fromUtc = request.FromUtc?.ToUniversalTime();
        var toUtc = request.ToUtc?.ToUniversalTime();

        var query = _movementRepository.Query()
            .AsNoTracking()
            .Where(x => x.BranchId == request.BranchId);

        if (request.ProductId.HasValue)
        {
            query = query.Where(x => x.ProductId == request.ProductId.Value);
        }

        if (request.MovementType.HasValue)
        {
            query = query.Where(x => x.MovementType == request.MovementType.Value);
        }

        if (fromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedOn >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            query = query.Where(x => x.CreatedOn <= toUtc.Value);
        }

        var skip = Math.Max(0, request.Skip);
        var take = Math.Clamp(request.Take, 1, 500);

        return await query
            .OrderByDescending(x => x.CreatedOn)
            .ThenByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new InventoryMovementHistoryItemDto
            {
                MovementId = x.Id,
                ProductId = x.ProductId,
                ProductName = x.Product != null ? x.Product.NameEn : string.Empty,
                MovementType = x.MovementType,
                QuantityChange = x.QuantityChange,
                ReferenceNumber = x.ReferenceNumber,
                Reason = x.Reason,
                CreatedOn = x.CreatedOn
            })
            .ToListAsync(cancellationToken);
    }
}
