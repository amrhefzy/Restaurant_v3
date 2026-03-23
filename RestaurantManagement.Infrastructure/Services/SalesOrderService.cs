using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.SalesOrders;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class SalesOrderService : ISalesOrderService
{
    private readonly IRepository<SalesOrder> _repository;
    private readonly IRepository<CashierShift> _shiftRepository;
    private readonly IRepository<PaymentTransaction> _paymentRepository;
    private readonly IRepository<RestaurantTable> _tableRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesOrderService(
        IRepository<SalesOrder> repository,
        IRepository<CashierShift> shiftRepository,
        IRepository<PaymentTransaction> paymentRepository,
        IRepository<RestaurantTable> tableRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _repository = repository;
        _shiftRepository = shiftRepository;
        _paymentRepository = paymentRepository;
        _tableRepository = tableRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<SalesOrderListItemDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _repository.Query()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(take)
            .ProjectTo<SalesOrderListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsPaidAsync(Guid branchId, Guid salesOrderId, PaymentType paymentType, CancellationToken cancellationToken = default)
    {
        var order = await _repository.Query()
            .FirstOrDefaultAsync(x => x.Id == salesOrderId && x.BranchId == branchId, cancellationToken)
            ?? throw new KeyNotFoundException("Sales order was not found for this branch.");

        if (order.PaymentStatus == PaymentStatus.Paid)
        {
            return;
        }

        if (order.Status == OrderStatus.Closed || order.Status == OrderStatus.Cancelled)
        {
            throw new ValidationException("Closed or cancelled orders cannot be paid.");
        }

        var shift = await _shiftRepository.Query()
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Status == ShiftStatus.Open, cancellationToken);
        if (shift is null)
        {
            throw new ValidationException("An open shift is required to record payment.");
        }

        var paidOn = DateTime.UtcNow;

        order.PaymentStatus = PaymentStatus.Paid;
        order.PaymentType = paymentType;
        order.PaidAmount = order.Total;
        order.PaidOnUtc = paidOn;
        order.CashierShiftId = shift.Id;
        _repository.Update(order);

        await _paymentRepository.AddAsync(new PaymentTransaction
        {
            BranchId = branchId,
            SalesOrderId = order.Id,
            CashierShiftId = shift.Id,
            PaymentType = paymentType,
            Amount = order.Total,
            PaidOnUtc = paidOn,
            ReferenceNumber = order.OrderNumber
        }, cancellationToken);

        shift.TotalSales = decimal.Round(shift.TotalSales + order.Total, 2, MidpointRounding.AwayFromZero);
        if (paymentType == PaymentType.Cash)
        {
            shift.TotalCash = decimal.Round(shift.TotalCash + order.Total, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            shift.TotalCard = decimal.Round(shift.TotalCard + order.Total, 2, MidpointRounding.AwayFromZero);
        }
        _shiftRepository.Update(shift);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CloseAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default)
    {
        var order = await _repository.Query()
            .FirstOrDefaultAsync(x => x.Id == salesOrderId && x.BranchId == branchId, cancellationToken)
            ?? throw new KeyNotFoundException("Sales order was not found for this branch.");

        if (order.PaymentStatus != PaymentStatus.Paid)
        {
            throw new ValidationException("Order cannot be closed before payment is completed.");
        }

        if (order.Status == OrderStatus.Closed)
        {
            return;
        }

        order.Status = OrderStatus.Closed;
        _repository.Update(order);

        if (order.OrderType == OrderType.DineIn && order.TableId.HasValue)
        {
            var table = await _tableRepository.GetByIdAsync(order.TableId.Value, cancellationToken);
            if (table is not null && table.BranchId == branchId)
            {
                table.Status = TableStatus.Free;
                _tableRepository.Update(table);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
