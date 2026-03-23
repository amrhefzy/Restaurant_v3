using RestaurantManagement.Application.DTOs.Tables;

namespace RestaurantManagement.Application.Services;

public interface ITableService
{
    Task<IReadOnlyCollection<TableDto>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<UpdateTableDto?> GetForEditAsync(Guid branchId, Guid id, CancellationToken cancellationToken = default);
    Task<TableDto> CreateAsync(CreateTableDto request, CancellationToken cancellationToken = default);
    Task<TableDto> UpdateAsync(UpdateTableDto request, CancellationToken cancellationToken = default);
}
