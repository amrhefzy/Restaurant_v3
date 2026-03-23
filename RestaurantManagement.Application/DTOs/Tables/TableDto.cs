using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Tables;

public sealed class TableDto
{
    public Guid Id { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public TableStatus Status { get; set; }
    public bool IsActive { get; set; }
}
