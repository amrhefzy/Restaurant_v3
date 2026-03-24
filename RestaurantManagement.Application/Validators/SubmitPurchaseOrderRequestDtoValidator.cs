using FluentValidation;
using RestaurantManagement.Application.DTOs.PurchaseOrders;

namespace RestaurantManagement.Application.Validators;

public sealed class SubmitPurchaseOrderRequestDtoValidator : AbstractValidator<SubmitPurchaseOrderRequestDto>
{
    public SubmitPurchaseOrderRequestDtoValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
    }
}
