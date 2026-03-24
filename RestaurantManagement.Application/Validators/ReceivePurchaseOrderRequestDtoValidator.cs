using FluentValidation;
using RestaurantManagement.Application.DTOs.PurchaseOrders;

namespace RestaurantManagement.Application.Validators;

public sealed class ReceivePurchaseOrderRequestDtoValidator : AbstractValidator<ReceivePurchaseOrderRequestDto>
{
    public ReceivePurchaseOrderRequestDtoValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new ReceivePurchaseOrderLineDtoValidator());
    }
}

public sealed class ReceivePurchaseOrderLineDtoValidator : AbstractValidator<ReceivePurchaseOrderLineDto>
{
    public ReceivePurchaseOrderLineDtoValidator()
    {
        RuleFor(x => x.PurchaseOrderItemId).NotEmpty();
        RuleFor(x => x.ReceivedQuantity).GreaterThan(0);
    }
}
