namespace RestaurantManagement.Application.DTOs.Shifts;

public sealed class OpenShiftRequestDto
{
    public Guid BranchId { get; set; }
    public decimal OpeningCash { get; set; }
    public string? OpenedByUserId { get; set; }
    public string? OpenedByUserName { get; set; }
}
