using RestaurantManagement.Application.DTOs.Customers;

namespace RestaurantManagement.Application.Services;

public interface ICustomerService
{
    Task<IReadOnlyCollection<CustomerDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<UpdateCustomerDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default);
    Task<CustomerDto> CreateAsync(CreateCustomerDto request, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(UpdateCustomerDto request, CancellationToken cancellationToken = default);
}
