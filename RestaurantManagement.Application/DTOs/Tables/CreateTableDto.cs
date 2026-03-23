namespace RestaurantManagement.Application.DTOs.Tables;

public sealed class CreateTableDto
{
    public Guid BranchId { get; set; }
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
}
