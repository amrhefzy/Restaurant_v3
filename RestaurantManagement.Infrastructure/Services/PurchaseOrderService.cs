using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.PurchaseOrders;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IRepository<PurchaseOrder> _repository;
    private readonly IMapper _mapper;

    public PurchaseOrderService(IRepository<PurchaseOrder> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<PurchaseOrderListItemDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _repository.Query()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(take)
            .ProjectTo<PurchaseOrderListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
