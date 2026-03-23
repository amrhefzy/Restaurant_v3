using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.Orders;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<RestaurantTable> _tableRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateOrderDto> _createValidator;
    private readonly IValidator<UpdateOrderDto> _updateValidator;

    public OrderService(
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IRepository<Customer> customerRepository,
        IRepository<RestaurantTable> tableRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateOrderDto> createValidator,
        IValidator<UpdateOrderDto> updateValidator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _tableRepository = tableRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _orderRepository.Query()
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(Math.Clamp(take, 1, 200))
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }

    public async Task<UpdateOrderDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _orderRepository.Query()
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && x.BranchId == branchId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var dto = _mapper.Map<UpdateOrderDto>(entity);
        dto.Items = entity.Items
            .OrderBy(x => x.Id)
            .Select(x => _mapper.Map<UpdateOrderItemDto>(x))
            .ToList();

        return dto;
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await ValidateHeaderReferencesAsync(request.BranchId, request.CustomerId, request.TableId, request.OrderType, cancellationToken);
        await ValidateProductsAsync(request.BranchId, request.Items.Select(x => x.ProductId), cancellationToken);

        var orderNumber = await GenerateOrderNumberAsync(request.BranchId, cancellationToken);
        var subtotal = 0m;
        var items = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var lineTotal = decimal.Round(item.Quantity * item.Price, 2, MidpointRounding.AwayFromZero);
            subtotal += lineTotal;

            items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                Total = lineTotal,
                Note = item.Note
            });
        }

        var order = new Order
        {
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            TableId = request.OrderType == OrderType.DineIn ? request.TableId : null,
            OrderNumber = orderNumber,
            OrderType = request.OrderType,
            Status = request.Status,
            Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            TaxAmount = decimal.Round(request.TaxAmount, 2, MidpointRounding.AwayFromZero),
            Total = decimal.Round(subtotal + request.TaxAmount, 2, MidpointRounding.AwayFromZero),
            Notes = request.Notes,
            Items = items
        };

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetProjectedByIdAsync(order.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Order was created but could not be loaded.");
    }

    public async Task<OrderDto> UpdateAsync(UpdateOrderDto request, CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await ValidateHeaderReferencesAsync(request.BranchId, request.CustomerId, request.TableId, request.OrderType, cancellationToken);
        await ValidateProductsAsync(request.BranchId, request.Items.Select(x => x.ProductId), cancellationToken);

        var order = await _orderRepository.Query()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Order was not found for this branch.");

        order.CustomerId = request.CustomerId;
        order.TableId = request.OrderType == OrderType.DineIn ? request.TableId : null;
        order.OrderType = request.OrderType;
        order.Status = request.Status;
        order.TaxAmount = decimal.Round(request.TaxAmount, 2, MidpointRounding.AwayFromZero);
        order.Notes = request.Notes;

        order.Items.Clear();
        var subtotal = 0m;
        foreach (var item in request.Items)
        {
            var lineTotal = decimal.Round(item.Quantity * item.Price, 2, MidpointRounding.AwayFromZero);
            subtotal += lineTotal;

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                Total = lineTotal,
                Note = item.Note
            });
        }

        order.Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero);
        order.Total = decimal.Round(order.Subtotal + order.TaxAmount, 2, MidpointRounding.AwayFromZero);

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetProjectedByIdAsync(order.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Order was updated but could not be loaded.");
    }

    private async Task ValidateHeaderReferencesAsync(
        Guid branchId,
        Guid? customerId,
        Guid? tableId,
        OrderType orderType,
        CancellationToken cancellationToken)
    {
        if (customerId.HasValue)
        {
            var customerExists = await _customerRepository.Query()
                .AnyAsync(x => x.Id == customerId.Value && x.BranchId == branchId && x.IsActive, cancellationToken);

            if (!customerExists)
            {
                throw new ValidationException("Selected customer is invalid for this branch.");
            }
        }

        if (orderType != OrderType.DineIn)
        {
            return;
        }

        if (!tableId.HasValue)
        {
            throw new ValidationException("Table is required for dine-in orders.");
        }

        var tableExists = await _tableRepository.Query()
            .AnyAsync(x => x.Id == tableId.Value && x.BranchId == branchId && x.IsActive, cancellationToken);

        if (!tableExists)
        {
            throw new ValidationException("Selected table is invalid for this branch.");
        }
    }

    private async Task ValidateProductsAsync(Guid branchId, IEnumerable<Guid> productIds, CancellationToken cancellationToken)
    {
        var ids = productIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            throw new ValidationException("At least one order item is required.");
        }

        var count = await _productRepository.Query()
            .CountAsync(x => x.BranchId == branchId && x.IsActive && ids.Contains(x.Id), cancellationToken);

        if (count != ids.Length)
        {
            throw new ValidationException("One or more products are invalid for this branch.");
        }
    }

    private async Task<string> GenerateOrderNumberAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var todayCount = await _orderRepository.Query()
            .CountAsync(x => x.BranchId == branchId && x.CreatedOn >= DateTime.UtcNow.Date, cancellationToken);

        return $"ORD-{datePart}-{todayCount + 1:0000}";
    }

    private Task<OrderDto?> GetProjectedByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return _orderRepository.Query()
            .AsNoTracking()
            .Where(x => x.Id == orderId)
            .ProjectTo<OrderDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
