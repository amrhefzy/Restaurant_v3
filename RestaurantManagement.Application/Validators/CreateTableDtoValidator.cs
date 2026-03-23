using FluentValidation;
using RestaurantManagement.Application.DTOs.Tables;

namespace RestaurantManagement.Application.Validators;

public sealed class CreateTableDtoValidator : AbstractValidator<CreateTableDto>
{
    public CreateTableDtoValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.TableNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 20);
    }
}
