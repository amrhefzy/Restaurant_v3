using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Reservations;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class ReservationService : IReservationService
{
    private readonly IRepository<Reservation> _repository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<RestaurantTable> _tableRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateReservationDto> _createValidator;
    private readonly IValidator<UpdateReservationDto> _updateValidator;

    public ReservationService(
        IRepository<Reservation> repository,
        IRepository<Customer> customerRepository,
        IRepository<RestaurantTable> tableRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateReservationDto> createValidator,
        IValidator<UpdateReservationDto> updateValidator)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _tableRepository = tableRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<ReservationDto>> GetUpcomingByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _repository.Query()
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderBy(x => x.ReservationAtUtc)
            .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<UpdateReservationDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.BranchId == branchId, cancellationToken);

        return entity is null ? null : _mapper.Map<UpdateReservationDto>(entity);
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await ValidateReferencesAsync(request.BranchId, request.CustomerId, request.TableId, cancellationToken);

        var entity = new Reservation
        {
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            TableId = request.TableId,
            ReservationAtUtc = request.ReservationAtUtc,
            PartySize = request.PartySize,
            Notes = request.Notes,
            Status = ReservationStatus.Confirmed
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectReservationByIdAsync(entity.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Reservation was created but could not be loaded.");
    }

    public async Task<ReservationDto> UpdateAsync(UpdateReservationDto request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await ValidateReferencesAsync(request.BranchId, request.CustomerId, request.TableId, cancellationToken);

        var entity = await _repository.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Reservation was not found for this branch.");

        _mapper.Map(request, entity);
        _repository.Update(entity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectReservationByIdAsync(entity.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Reservation was updated but could not be loaded.");
    }

    private async Task ValidateReferencesAsync(Guid branchId, Guid customerId, Guid? tableId, CancellationToken cancellationToken)
    {
        var hasCustomer = await _customerRepository.Query()
            .AnyAsync(x => x.Id == customerId && x.BranchId == branchId && x.IsActive, cancellationToken);

        if (!hasCustomer)
        {
            throw new ValidationException("Selected customer is invalid for this branch.");
        }

        if (!tableId.HasValue)
        {
            return;
        }

        var hasTable = await _tableRepository.Query()
            .AnyAsync(x => x.Id == tableId.Value && x.BranchId == branchId && x.IsActive, cancellationToken);

        if (!hasTable)
        {
            throw new ValidationException("Selected table is invalid for this branch.");
        }
    }

    private Task<ReservationDto?> ProjectReservationByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _repository.Query()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .ProjectTo<ReservationDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
