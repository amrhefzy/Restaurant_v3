using FluentValidation;
using RestaurantManagement.Application.DTOs.POS;
using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.Validators;

public sealed class PosCheckoutRequestDtoValidator : AbstractValidator<PosCheckoutRequestDto>
{
    public PosCheckoutRequestDtoValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new PosCheckoutItemDtoValidator());
        RuleFor(x => x.OrderType).IsInEnum();
        RuleFor(x => x.PaymentType)
            .NotNull()
            .When(x => !x.IsHold)
            .WithMessage("Payment type is required for checkout.");
        RuleFor(x => x.PaymentType)
            .IsInEnum()
            .When(x => x.PaymentType.HasValue);
        RuleFor(x => x.DiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TableId)
            .NotEmpty()
            .When(x => x.OrderType == OrderType.DineIn)
            .WithMessage("Table is required for dine-in orders.");
    }
}

public sealed class PosCheckoutItemDtoValidator : AbstractValidator<PosCheckoutItemDto>
{
    public PosCheckoutItemDtoValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
