using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Tables;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class TableService : ITableService
{
    private readonly IRepository<RestaurantTable> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTableDto> _createValidator;
    private readonly IValidator<UpdateTableDto> _updateValidator;

    public TableService(
        IRepository<RestaurantTable> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateTableDto> createValidator,
        IValidator<UpdateTableDto> updateValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<TableDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _repository.Query()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.TableNumber)
            .ProjectTo<TableDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<UpdateTableDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.BranchId == branchId, cancellationToken);

        return entity is null ? null : _mapper.Map<UpdateTableDto>(entity);
    }

    public async Task<TableDto> CreateAsync(CreateTableDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = _mapper.Map<RestaurantTable>(request);
        entity.Status = TableStatus.Free;

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TableDto>(entity);
    }

    public async Task<TableDto> UpdateAsync(UpdateTableDto request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _repository.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Table was not found for this branch.");

        _mapper.Map(request, entity);
        _repository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TableDto>(entity);
    }
}
