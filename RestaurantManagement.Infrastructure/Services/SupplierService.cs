using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Suppliers;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateSupplierDto> _createValidator;
    private readonly IValidator<UpdateSupplierDto> _updateValidator;

    public SupplierService(
        IRepository<Supplier> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateSupplierDto> createValidator,
        IValidator<UpdateSupplierDto> updateValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<SupplierDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _repository.Query()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.Name)
            .ProjectTo<SupplierDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<UpdateSupplierDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.BranchId == branchId, cancellationToken);

        return entity is null ? null : _mapper.Map<UpdateSupplierDto>(entity);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = _mapper.Map<Supplier>(request);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SupplierDto>(entity);
    }

    public async Task<SupplierDto> UpdateAsync(UpdateSupplierDto request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _repository.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Supplier was not found for this branch.");

        _mapper.Map(request, entity);
        _repository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SupplierDto>(entity);
    }
}
