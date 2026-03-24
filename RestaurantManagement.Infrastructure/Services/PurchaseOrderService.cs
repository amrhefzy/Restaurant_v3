using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Application.DTOs.PurchaseOrders;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IRepository<PurchaseOrder> _purchaseOrderRepository;
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<InventoryMovement> _inventoryMovementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreatePurchaseOrderDto> _createValidator;
    private readonly IValidator<SubmitPurchaseOrderRequestDto> _submitValidator;
    private readonly IValidator<ReceivePurchaseOrderRequestDto> _receiveValidator;

    public PurchaseOrderService(
        IRepository<PurchaseOrder> purchaseOrderRepository,
        IRepository<Supplier> supplierRepository,
        IRepository<Product> productRepository,
        IRepository<InventoryMovement> inventoryMovementRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreatePurchaseOrderDto> createValidator,
        IValidator<SubmitPurchaseOrderRequestDto> submitValidator,
        IValidator<ReceivePurchaseOrderRequestDto> receiveValidator)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _supplierRepository = supplierRepository;
        _productRepository = productRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _submitValidator = submitValidator;
        _receiveValidator = receiveValidator;
    }

    public async Task<IReadOnlyCollection<PurchaseOrderListItemDto>> GetRecentByBranchAsync(Guid branchId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _purchaseOrderRepository.Query()
            .AsNoTracking()
            .Where(x => x.BranchId == branchId)
            .OrderByDescending(x => x.CreatedOn)
            .Take(Math.Clamp(take, 1, 200))
            .Select(x => new PurchaseOrderListItemDto
            {
                Id = x.Id,
                PurchaseOrderNumber = x.PurchaseOrderNumber,
                CreatedOn = x.CreatedOn,
                Status = x.Status,
                Total = x.Total
            })
            .ToListAsync(cancellationToken);
    }

    public Task<PurchaseOrderDetailsDto?> GetByIdAsync(Guid branchId, Guid purchaseOrderId, CancellationToken cancellationToken = default)
        => ProjectDetailsByIdAsync(branchId, purchaseOrderId, cancellationToken);

    public async Task<PurchaseOrderDetailsDto> CreateDraftAsync(CreatePurchaseOrderDto request, CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var supplierExists = await _supplierRepository.Query()
            .AnyAsync(x => x.Id == request.SupplierId && x.BranchId == request.BranchId && x.IsActive, cancellationToken);
        if (!supplierExists)
        {
            throw new ValidationException("Selected supplier is invalid for this branch.");
        }

        var requestItems = request.Items
            .GroupBy(x => x.ProductId)
            .Select(x => new
            {
                ProductId = x.Key,
                Quantity = x.Sum(i => i.Quantity),
                UnitCost = x.Last().UnitCost
            })
            .ToArray();

        var productIds = requestItems.Select(x => x.ProductId).ToArray();
        var products = await _productRepository.Query()
            .Where(x => x.BranchId == request.BranchId && x.IsActive && productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.NameEn })
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Length)
        {
            throw new ValidationException("One or more products are invalid for this branch.");
        }

        var items = new List<PurchaseOrderItem>();
        decimal subtotal = 0m;

        foreach (var item in requestItems)
        {
            var lineTotal = decimal.Round(item.Quantity * item.UnitCost, 2, MidpointRounding.AwayFromZero);
            subtotal = decimal.Round(subtotal + lineTotal, 2, MidpointRounding.AwayFromZero);

            items.Add(new PurchaseOrderItem
            {
                ProductId = item.ProductId,
                Quantity = decimal.Round(item.Quantity, 3, MidpointRounding.AwayFromZero),
                ReceivedQuantity = 0m,
                UnitCost = decimal.Round(item.UnitCost, 2, MidpointRounding.AwayFromZero),
                LineTotal = lineTotal
            });
        }

        var taxAmount = decimal.Round(request.TaxAmount, 2, MidpointRounding.AwayFromZero);
        var order = new PurchaseOrder
        {
            BranchId = request.BranchId,
            SupplierId = request.SupplierId,
            PurchaseOrderNumber = await GeneratePurchaseOrderNumberAsync(request.BranchId, cancellationToken),
            Status = PurchaseOrderStatus.Draft,
            Notes = request.Notes,
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            Total = decimal.Round(subtotal + taxAmount, 2, MidpointRounding.AwayFromZero),
            Items = items
        };

        await _purchaseOrderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectDetailsByIdAsync(request.BranchId, order.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase order was created but could not be loaded.");
    }

    public async Task<PurchaseOrderDetailsDto> SubmitAsync(SubmitPurchaseOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        await _submitValidator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _purchaseOrderRepository.Query()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.PurchaseOrderId && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase order was not found for this branch.");

        if (order.Status == PurchaseOrderStatus.Cancelled)
        {
            throw new ValidationException("Cancelled purchase orders cannot be submitted.");
        }

        if (order.Status is PurchaseOrderStatus.PartiallyReceived or PurchaseOrderStatus.Received)
        {
            throw new ValidationException("Purchase order is already in receiving workflow.");
        }

        if (order.Items.Count == 0)
        {
            throw new ValidationException("Purchase order must contain at least one item before submission.");
        }

        order.Status = PurchaseOrderStatus.Submitted;
        order.SubmittedOnUtc = DateTime.UtcNow;
        _purchaseOrderRepository.Update(order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await ProjectDetailsByIdAsync(request.BranchId, order.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase order was submitted but could not be loaded.");
    }

    public async Task<PurchaseOrderDetailsDto> ReceiveAsync(ReceivePurchaseOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        await _receiveValidator.ValidateAndThrowAsync(request, cancellationToken);

        var order = await _purchaseOrderRepository.Query()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == request.PurchaseOrderId && x.BranchId == request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase order was not found for this branch.");

        if (order.Status == PurchaseOrderStatus.Draft)
        {
            throw new ValidationException("Purchase order must be submitted before receiving.");
        }

        if (order.Status == PurchaseOrderStatus.Cancelled)
        {
            throw new ValidationException("Cancelled purchase orders cannot be received.");
        }

        if (order.Status == PurchaseOrderStatus.Received)
        {
            throw new ValidationException("Purchase order is already fully received.");
        }

        var receiptLines = request.Items
            .GroupBy(x => x.PurchaseOrderItemId)
            .Select(x => new
            {
                PurchaseOrderItemId = x.Key,
                ReceivedQuantity = decimal.Round(x.Sum(i => i.ReceivedQuantity), 3, MidpointRounding.AwayFromZero)
            })
            .ToArray();

        var orderItemsById = order.Items.ToDictionary(x => x.Id);
        foreach (var line in receiptLines)
        {
            if (!orderItemsById.TryGetValue(line.PurchaseOrderItemId, out var orderItem))
            {
                throw new ValidationException($"Invalid purchase order item id: {line.PurchaseOrderItemId}");
            }

            var remaining = decimal.Round(orderItem.Quantity - orderItem.ReceivedQuantity, 3, MidpointRounding.AwayFromZero);
            if (line.ReceivedQuantity > remaining)
            {
                throw new ValidationException("Received quantity cannot exceed remaining quantity.");
            }
        }

        var referenceNumber = string.IsNullOrWhiteSpace(request.ReceiptReferenceNumber)
            ? $"GRN-{order.PurchaseOrderNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.ReceiptReferenceNumber.Trim();
        var reason = string.IsNullOrWhiteSpace(request.Notes) ? "Purchase order receiving" : request.Notes.Trim();

        var duplicateReceiptExists = await _inventoryMovementRepository.Query()
            .AnyAsync(x =>
                x.BranchId == request.BranchId
                && x.MovementType == InventoryMovementType.PurchaseReceipt
                && x.ReferenceNumber == referenceNumber,
                cancellationToken);

        if (duplicateReceiptExists)
        {
            throw new ValidationException("Receipt reference already exists. Please use a unique reference.");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var line in receiptLines)
            {
                var orderItem = orderItemsById[line.PurchaseOrderItemId];
                if (line.ReceivedQuantity <= 0)
                {
                    continue;
                }

                orderItem.ReceivedQuantity = decimal.Round(orderItem.ReceivedQuantity + line.ReceivedQuantity, 3, MidpointRounding.AwayFromZero);

                await _inventoryMovementRepository.AddAsync(new InventoryMovement
                {
                    BranchId = request.BranchId,
                    ProductId = orderItem.ProductId,
                    MovementType = InventoryMovementType.PurchaseReceipt,
                    QuantityChange = line.ReceivedQuantity,
                    ReferenceNumber = referenceNumber,
                    Reason = reason
                }, ct);
            }

            var isFullyReceived = order.Items.All(x => x.ReceivedQuantity >= x.Quantity);
            order.Status = isFullyReceived ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
            _purchaseOrderRepository.Update(order);

            await _unitOfWork.SaveChangesAsync(ct);
        }, cancellationToken);

        return await ProjectDetailsByIdAsync(request.BranchId, order.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Purchase order receipt was saved but could not be loaded.");
    }

    private async Task<string> GeneratePurchaseOrderNumberAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var todayStart = DateTime.UtcNow.Date;
        var todayCount = await _purchaseOrderRepository.Query()
            .CountAsync(x => x.BranchId == branchId && x.CreatedOn >= todayStart, cancellationToken);

        return $"PO-{datePart}-{todayCount + 1:0000}";
    }

    private Task<PurchaseOrderDetailsDto?> ProjectDetailsByIdAsync(Guid branchId, Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        return _purchaseOrderRepository.Query()
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.Id == purchaseOrderId)
            .Select(x => new PurchaseOrderDetailsDto
            {
                Id = x.Id,
                BranchId = x.BranchId,
                SupplierId = x.SupplierId,
                SupplierName = x.Supplier != null ? x.Supplier.Name : string.Empty,
                PurchaseOrderNumber = x.PurchaseOrderNumber,
                Status = x.Status,
                CreatedOn = x.CreatedOn,
                SubmittedOnUtc = x.SubmittedOnUtc,
                Notes = x.Notes,
                Subtotal = x.Subtotal,
                TaxAmount = x.TaxAmount,
                Total = x.Total,
                Items = x.Items
                    .OrderBy(i => i.Id)
                    .Select(i => new PurchaseOrderItemDetailsDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product != null ? i.Product.NameEn : string.Empty,
                        Quantity = i.Quantity,
                        ReceivedQuantity = i.ReceivedQuantity,
                        RemainingQuantity = i.Quantity - i.ReceivedQuantity,
                        UnitCost = i.UnitCost,
                        LineTotal = i.LineTotal
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
