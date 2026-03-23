using FluentValidation;
using RestaurantManagement.Application.DTOs.Shifts;

namespace RestaurantManagement.Application.Validators;

public sealed class CloseShiftRequestDtoValidator : AbstractValidator<CloseShiftRequestDto>
{
    public CloseShiftRequestDtoValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.ClosingCashActual).GreaterThanOrEqualTo(0);
    }
}
