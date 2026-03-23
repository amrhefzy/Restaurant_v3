using RestaurantManagement.Application.DTOs.Products;

namespace RestaurantManagement.Application.Services;

public interface IProductService
{
    Task<IReadOnlyCollection<ProductDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<UpdateProductDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto request, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(UpdateProductDto request, CancellationToken cancellationToken = default);
}
