using RestaurantManagement.Application.DTOs.Categories;

namespace RestaurantManagement.Application.Services;

public interface ICategoryService
{
    Task<IReadOnlyCollection<CategoryDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<CreateCategoryDto?> GetForCreateAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<UpdateCategoryDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto request, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(UpdateCategoryDto request, CancellationToken cancellationToken = default);
}
