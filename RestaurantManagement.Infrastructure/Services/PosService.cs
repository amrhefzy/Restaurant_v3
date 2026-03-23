using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.DTOs.Categories;
using RestaurantManagement.Application.DTOs.POS;
using RestaurantManagement.Application.DTOs.Tables;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class PosService : IPosService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<PosCheckoutRequestDto> _validator;

    public PosService(AppDbContext dbContext, IMapper mapper, IValidator<PosCheckoutRequestDto> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PosScreenDataDto> GetScreenDataAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsActive)
            .OrderBy(x => x.NameEn)
            .ProjectTo<PosProductDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var tables = await _dbContext.RestaurantTables
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.IsActive)
            .OrderBy(x => x.TableNumber)
            .ProjectTo<TableDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var heldOrders = await _dbContext.SalesOrders
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.Status == OrderStatus.Held && x.HeldReference != null)
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => new PosHeldOrderDto
            {
                SalesOrderId = x.Id,
                HeldReference = x.HeldReference ?? string.Empty,
                Total = x.Total,
                CreatedOn = x.CreatedOn
            })
            .Take(20)
            .ToListAsync(cancellationToken);

        var activeOrders = await GetActiveOrderStatusesAsync(branchId, cancellationToken);

        return new PosScreenDataDto
        {
            Categories = categories,
            Products = products,
            Tables = tables,
            HeldOrders = heldOrders,
            ActiveOrders = activeOrders
        };
    }

    public async Task<PosCheckoutResponseDto> ProcessOrderAsync(PosCheckoutRequestDto request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        await ValidateOrderContextAsync(request, cancellationToken);

        CashierShift? openShift = null;
        if (!request.IsHold)
        {
            if (!request.PaymentType.HasValue)
            {
                throw new ValidationException("Payment type is required for checkout.");
            }

            openShift = await _dbContext.CashierShifts
                .FirstOrDefaultAsync(x => x.BranchId == request.BranchId && x.Status == ShiftStatus.Open, cancellationToken);

            if (openShift is null)
            {
                throw new ValidationException("An open shift is required before checkout.");
            }
        }

        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToArray();
        var products = await _dbContext.Products
            .Where(x => x.BranchId == request.BranchId && productIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        if (products.Count != productIds.Length)
        {
            throw new ValidationException("One or more selected products are invalid for this branch.");
        }

        var subtotal = 0m;
        var orderItems = new List<SalesOrderItem>();

        foreach (var item in request.Items)
        {
            var product = products[item.ProductId];
            var lineTotal = decimal.Round(product.SalePrice * item.Quantity, 2, MidpointRounding.AwayFromZero);
            subtotal += lineTotal;

            orderItems.Add(new SalesOrderItem
            {
                ProductId = product.Id,
                ProductName = product.NameEn,
                Quantity = item.Quantity,
                UnitPrice = product.SalePrice,
                LineTotal = lineTotal,
                TaxAmount = 0m,
                DiscountAmount = 0m,
                Note = item.Note
            });
        }

        var total = subtotal + request.TaxAmount - request.DiscountAmount;
        var orderNumber = await GenerateOrderNumberAsync(request.BranchId, cancellationToken);
        var heldReference = request.IsHold
            ? (string.IsNullOrWhiteSpace(request.HeldReference) ? $"HOLD-{DateTime.UtcNow:yyyyMMddHHmmss}" : request.HeldReference)
            : null;

        var order = new SalesOrder
        {
            BranchId = request.BranchId,
            CustomerId = request.CustomerId,
            TableId = request.OrderType == OrderType.DineIn ? request.TableId : null,
            OrderNumber = orderNumber,
            OrderType = request.OrderType,
            Status = request.IsHold ? OrderStatus.Held : OrderStatus.New,
            PaymentStatus = request.IsHold ? PaymentStatus.Pending : PaymentStatus.Paid,
            PaymentType = request.IsHold ? null : request.PaymentType,
            PaidAmount = request.IsHold ? 0m : decimal.Round(total, 2, MidpointRounding.AwayFromZero),
            PaidOnUtc = request.IsHold ? null : DateTime.UtcNow,
            CashierShiftId = request.IsHold ? null : openShift!.Id,
            Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero),
            TaxAmount = decimal.Round(request.TaxAmount, 2, MidpointRounding.AwayFromZero),
            DiscountAmount = decimal.Round(request.DiscountAmount, 2, MidpointRounding.AwayFromZero),
            Total = decimal.Round(total, 2, MidpointRounding.AwayFromZero),
            Notes = request.Notes,
            HeldReference = heldReference,
            Items = orderItems
        };

        _dbContext.SalesOrders.Add(order);

        if (!request.IsHold)
        {
            _dbContext.PaymentTransactions.Add(new PaymentTransaction
            {
                BranchId = request.BranchId,
                SalesOrderId = order.Id,
                CashierShiftId = openShift!.Id,
                PaymentType = request.PaymentType!.Value,
                Amount = order.Total,
                PaidOnUtc = DateTime.UtcNow,
                ReferenceNumber = order.OrderNumber
            });

            openShift.TotalSales = decimal.Round(openShift.TotalSales + order.Total, 2, MidpointRounding.AwayFromZero);
            if (request.PaymentType == PaymentType.Cash)
            {
                openShift.TotalCash = decimal.Round(openShift.TotalCash + order.Total, 2, MidpointRounding.AwayFromZero);
            }
            else
            {
                openShift.TotalCard = decimal.Round(openShift.TotalCard + order.Total, 2, MidpointRounding.AwayFromZero);
            }

            if (request.OrderType == OrderType.DineIn && request.TableId.HasValue)
            {
                var table = await _dbContext.RestaurantTables
                    .FirstOrDefaultAsync(x => x.Id == request.TableId.Value && x.BranchId == request.BranchId, cancellationToken);
                if (table is not null)
                {
                    table.Status = TableStatus.Occupied;
                }
            }

            foreach (var item in orderItems)
            {
                var product = products[item.ProductId];
                if (!product.IsStockTracked)
                {
                    continue;
                }

                _dbContext.InventoryMovements.Add(new InventoryMovement
                {
                    BranchId = request.BranchId,
                    ProductId = product.Id,
                    MovementType = InventoryMovementType.SaleIssue,
                    QuantityChange = -item.Quantity,
                    ReferenceNumber = orderNumber,
                    Reason = "POS sale checkout"
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PosCheckoutResponseDto
        {
            SalesOrderId = order.Id,
            OrderNumber = order.OrderNumber,
            Subtotal = order.Subtotal,
            TaxAmount = order.TaxAmount,
            DiscountAmount = order.DiscountAmount,
            Total = order.Total,
            Status = order.Status.ToString(),
            HeldReference = order.HeldReference
        };
    }

    public async Task<PosHeldOrderDetailsDto?> GetHeldOrderAsync(Guid branchId, Guid salesOrderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesOrders
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.Id == salesOrderId && x.Status == OrderStatus.Held)
            .Select(x => new PosHeldOrderDetailsDto
            {
                SalesOrderId = x.Id,
                OrderNumber = x.OrderNumber,
                HeldReference = x.HeldReference,
                OrderType = x.OrderType,
                TableId = x.TableId,
                Notes = x.Notes,
                Items = x.Items.Select(i => new PosHeldOrderItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Note = i.Note
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<PosOrderStatusDto>> GetActiveOrderStatusesAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SalesOrders
            .AsNoTracking()
            .Where(x => x.BranchId == branchId
                        && (x.Status == OrderStatus.New
                            || x.Status == OrderStatus.InKitchen
                            || x.Status == OrderStatus.Ready
                            || x.Status == OrderStatus.Served))
            .OrderByDescending(x => x.CreatedOn)
            .Take(30)
            .Select(x => new PosOrderStatusDto
            {
                SalesOrderId = x.Id,
                OrderNumber = x.OrderNumber,
                Status = x.Status.ToString(),
                CreatedOn = x.CreatedOn,
                IsReady = x.Status == OrderStatus.Ready
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<string> GenerateOrderNumberAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var todayCount = await _dbContext.SalesOrders
            .CountAsync(x => x.BranchId == branchId && x.CreatedOn >= DateTime.UtcNow.Date, cancellationToken);

        return $"SO-{datePart}-{todayCount + 1:0000}";
    }

    private async Task ValidateOrderContextAsync(PosCheckoutRequestDto request, CancellationToken cancellationToken)
    {
        if (request.OrderType == OrderType.DineIn && !request.TableId.HasValue)
        {
            throw new ValidationException("Table is required for dine-in orders.");
        }

        if (request.TableId.HasValue)
        {
            var tableExists = await _dbContext.RestaurantTables
                .AnyAsync(x => x.Id == request.TableId.Value && x.BranchId == request.BranchId && x.IsActive, cancellationToken);

            if (!tableExists)
            {
                throw new ValidationException("Selected table is invalid for this branch.");
            }
        }

        if (request.CustomerId.HasValue)
        {
            var customerExists = await _dbContext.Customers
                .AnyAsync(x => x.Id == request.CustomerId.Value && x.BranchId == request.BranchId && x.IsActive, cancellationToken);

            if (!customerExists)
            {
                throw new ValidationException("Selected customer is invalid for this branch.");
            }
        }
    }
}
