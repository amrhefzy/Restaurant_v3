namespace RestaurantManagement.Application.DTOs.Shifts;

public sealed class CloseShiftRequestDto
{
    public Guid BranchId { get; set; }
    public decimal ClosingCashActual { get; set; }
    public string? ClosedByUserId { get; set; }
    public string? ClosedByUserName { get; set; }
}
