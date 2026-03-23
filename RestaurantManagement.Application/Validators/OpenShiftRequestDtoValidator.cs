using FluentValidation;
using RestaurantManagement.Application.DTOs.Shifts;

namespace RestaurantManagement.Application.Validators;

public sealed class OpenShiftRequestDtoValidator : AbstractValidator<OpenShiftRequestDto>
{
    public OpenShiftRequestDtoValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.OpeningCash).GreaterThanOrEqualTo(0);
    }
}
