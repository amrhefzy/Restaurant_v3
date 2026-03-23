using FluentValidation;
using RestaurantManagement.Application.DTOs.Categories;

namespace RestaurantManagement.Application.Validators;

public sealed class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.NameEn).NotEmpty().MaximumLength(120);
        RuleFor(x => x.NameAr).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
