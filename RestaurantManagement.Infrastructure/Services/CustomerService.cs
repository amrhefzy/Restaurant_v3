using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Customers;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly IRepository<Customer> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCustomerDto> _createValidator;
    private readonly IValidator<UpdateCustomerDto> _updateValidator;

    public CustomerService(
        IRepository<Customer> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateCustomerDto> createValidator,
        IValidator<UpdateCustomerDto> updateValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<CustomerDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _repository.Query()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.Name)
            .ProjectTo<CustomerDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<UpdateCustomerDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.BranchId == branchId, cancellationToken);

        return entity is null ? null : _mapper.Map<UpdateCustomerDto>(entity);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = _mapper.Map<Customer>(request);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(entity);
    }

    public async Task<CustomerDto> UpdateAsync(UpdateCustomerDto request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var entity = await _repository.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Customer was not found for this branch.");

        _mapper.Map(request, entity);
        _repository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CustomerDto>(entity);
    }
}
