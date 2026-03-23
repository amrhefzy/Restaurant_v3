using FluentValidation;
using RestaurantManagement.Application.DTOs.Reservations;

namespace RestaurantManagement.Application.Validators;

public sealed class UpdateReservationDtoValidator : AbstractValidator<UpdateReservationDto>
{
    public UpdateReservationDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.PartySize).InclusiveBetween(1, 20);
        RuleFor(x => x.ReservationAtUtc).GreaterThan(DateTime.UtcNow.AddMinutes(-5));
    }
}
