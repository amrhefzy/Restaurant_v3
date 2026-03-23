using RestaurantManagement.Domain.Enums;

namespace RestaurantManagement.Application.DTOs.Shifts;

public sealed class ShiftSummaryDto
{
    public Guid Id { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public ShiftStatus Status { get; set; }
    public DateTime OpenedOnUtc { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalCard { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? ClosingCashActual { get; set; }
    public decimal? CashVariance { get; set; }
    public DateTime? ClosedOnUtc { get; set; }
    public string? OpenedByUserName { get; set; }
    public string? ClosedByUserName { get; set; }
}
