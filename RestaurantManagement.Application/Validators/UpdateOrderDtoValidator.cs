using FluentValidation;
using RestaurantManagement.Application.DTOs.Orders;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.Validators;

public sealed class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OrderType).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new UpdateOrderItemDtoValidator());

        RuleFor(x => x.TableId)
            .NotEmpty()
            .When(x => x.OrderType == OrderType.DineIn)
            .WithMessage("Table is required for dine-in orders.");
    }
}
