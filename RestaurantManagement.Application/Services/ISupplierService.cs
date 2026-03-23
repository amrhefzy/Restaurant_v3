using RestaurantManagement.Application.DTOs.Suppliers;

namespace RestaurantManagement.Application.Services;

public interface ISupplierService
{
    Task<IReadOnlyCollection<SupplierDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<UpdateSupplierDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateAsync(CreateSupplierDto request, CancellationToken cancellationToken = default);
    Task<SupplierDto> UpdateAsync(UpdateSupplierDto request, CancellationToken cancellationToken = default);
}
