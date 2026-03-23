using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.DTOs.Shifts;
using RestaurantManagement.Application.Services;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Domain.Enums;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Infrastructure.Services;

public sealed class ShiftService : IShiftService
{
    private readonly AppDbContext _dbContext;
    private readonly IValidator<OpenShiftRequestDto> _openValidator;
    private readonly IValidator<CloseShiftRequestDto> _closeValidator;

    public ShiftService(
        AppDbContext dbContext,
        IValidator<OpenShiftRequestDto> openValidator,
        IValidator<CloseShiftRequestDto> closeValidator)
    {
        _dbContext = dbContext;
        _openValidator = openValidator;
        _closeValidator = closeValidator;
    }

    public async Task<ShiftSummaryDto?> GetCurrentAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        var shift = await _dbContext.CashierShifts
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.Status == ShiftStatus.Open)
            .OrderByDescending(x => x.OpenedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return shift is null ? null : MapSummary(shift);
    }

    public async Task<ShiftSummaryDto> OpenAsync(OpenShiftRequestDto request, CancellationToken cancellationToken = default)
    {
        await _openValidator.ValidateAndThrowAsync(request, cancellationToken);

        var hasOpenShift = await _dbContext.CashierShifts
            .AnyAsync(x => x.BranchId == request.BranchId && x.Status == ShiftStatus.Open, cancellationToken);
        if (hasOpenShift)
        {
            throw new ValidationException("An open shift already exists for this branch.");
        }

        var shift = new CashierShift
        {
            BranchId = request.BranchId,
            ShiftNumber = await GenerateShiftNumberAsync(request.BranchId, cancellationToken),
            Status = ShiftStatus.Open,
            OpeningCash = decimal.Round(request.OpeningCash, 2, MidpointRounding.AwayFromZero),
            OpenedOnUtc = DateTime.UtcNow,
            OpenedByUserId = request.OpenedByUserId,
            OpenedByUserName = request.OpenedByUserName
        };

        _dbContext.CashierShifts.Add(shift);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapSummary(shift);
    }

    public async Task<ShiftSummaryDto> CloseAsync(CloseShiftRequestDto request, CancellationToken cancellationToken = default)
    {
        await _closeValidator.ValidateAndThrowAsync(request, cancellationToken);

        var shift = await _dbContext.CashierShifts
            .FirstOrDefaultAsync(x => x.BranchId == request.BranchId && x.Status == ShiftStatus.Open, cancellationToken);

        if (shift is null)
        {
            throw new ValidationException("No open shift found for this branch.");
        }

        var expectedCash = decimal.Round(shift.OpeningCash + shift.TotalCash, 2, MidpointRounding.AwayFromZero);
        var closingActual = decimal.Round(request.ClosingCashActual, 2, MidpointRounding.AwayFromZero);

        shift.ClosingCashExpected = expectedCash;
        shift.ClosingCashActual = closingActual;
        shift.CashVariance = decimal.Round(closingActual - expectedCash, 2, MidpointRounding.AwayFromZero);
        shift.ClosedOnUtc = DateTime.UtcNow;
        shift.ClosedByUserId = request.ClosedByUserId;
        shift.ClosedByUserName = request.ClosedByUserName;
        shift.Status = ShiftStatus.Closed;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapSummary(shift);
    }

    private async Task<string> GenerateShiftNumberAsync(Guid branchId, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var count = await _dbContext.CashierShifts
            .CountAsync(x => x.BranchId == branchId && x.OpenedOnUtc >= today, cancellationToken);

        return $"SH-{today:yyyyMMdd}-{count + 1:000}";
    }

    private static ShiftSummaryDto MapSummary(CashierShift shift)
    {
        return new ShiftSummaryDto
        {
            Id = shift.Id,
            ShiftNumber = shift.ShiftNumber,
            Status = shift.Status,
            OpenedOnUtc = shift.OpenedOnUtc,
            OpeningCash = shift.OpeningCash,
            TotalSales = shift.TotalSales,
            TotalCash = shift.TotalCash,
            TotalCard = shift.TotalCard,
            ExpectedCash = shift.ClosingCashExpected ?? decimal.Round(shift.OpeningCash + shift.TotalCash, 2, MidpointRounding.AwayFromZero),
            ClosingCashActual = shift.ClosingCashActual,
            CashVariance = shift.ClosedOnUtc.HasValue ? shift.CashVariance : null,
            ClosedOnUtc = shift.ClosedOnUtc,
            OpenedByUserName = shift.OpenedByUserName,
            ClosedByUserName = shift.ClosedByUserName
        };
    }
}
