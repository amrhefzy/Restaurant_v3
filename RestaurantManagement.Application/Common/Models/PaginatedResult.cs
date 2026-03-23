namespace RestaurantManagement.Application.Common.Models;

public sealed class PaginatedResult<T>
{
    public int TotalCount { get; set; }
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
}
