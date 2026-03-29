using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Identity;
using RestaurantManagement.Web.Configuration;

namespace RestaurantManagement.Web.Services;

public sealed class SettingsRuntimeService : ISettingsRuntimeService
{
    private readonly IRepository<AppSetting> _settingsRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public SettingsRuntimeService(
        IRepository<AppSetting> settingsRepository,
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUserService)
    {
        _settingsRepository = settingsRepository;
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<bool> IsRoleAwareNavigationEnabledAsync(CancellationToken cancellationToken = default)
        => await GetBoolSettingAsync(SettingsCatalog.Keys.RoleAwareNavigationEnabled, SettingsCatalog.Defaults.RoleAwareNavigationEnabled, cancellationToken);

    public async Task<bool> IsLoginRequiredForOperationsAsync(CancellationToken cancellationToken = default)
        => await GetBoolSettingAsync(SettingsCatalog.Keys.RequireLoginForOperations, SettingsCatalog.Defaults.RequireLoginForOperations, cancellationToken);

    public async Task<bool> IsInventoryTrackingEnabledAsync(CancellationToken cancellationToken = default)
        => await GetBoolSettingAsync(SettingsCatalog.Keys.TrackInventory, SettingsCatalog.Defaults.TrackInventory, cancellationToken);

    public async Task<string> GetCurrencyCodeAsync(CancellationToken cancellationToken = default)
        => await GetStringSettingAsync(SettingsCatalog.Keys.CurrencyCode, SettingsCatalog.Defaults.CurrencyCode, cancellationToken);

    public async Task<decimal> GetTaxRatePercentAsync(CancellationToken cancellationToken = default)
        => await GetDecimalSettingAsync(SettingsCatalog.Keys.TaxRatePercent, SettingsCatalog.Defaults.TaxRatePercent, cancellationToken);

    public async Task<decimal> GetServiceChargePercentAsync(CancellationToken cancellationToken = default)
        => await GetDecimalSettingAsync(SettingsCatalog.Keys.ServiceChargePercent, SettingsCatalog.Defaults.ServiceChargePercent, cancellationToken);

    private async Task<bool> GetBoolSettingAsync(string key, bool fallback, CancellationToken cancellationToken)
    {
        var branchId = await GetCurrentBranchIdAsync(cancellationToken);
        if (branchId is null || branchId == Guid.Empty)
        {
            return fallback;
        }

        var setting = await _settingsRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Key == key, cancellationToken);

        return bool.TryParse(setting?.Value, out var parsed) ? parsed : fallback;
    }

    private async Task<string> GetStringSettingAsync(string key, string fallback, CancellationToken cancellationToken)
    {
        var branchId = await GetCurrentBranchIdAsync(cancellationToken);
        if (branchId is null || branchId == Guid.Empty)
        {
            return fallback;
        }

        var setting = await _settingsRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Key == key, cancellationToken);

        return string.IsNullOrWhiteSpace(setting?.Value) ? fallback : setting.Value;
    }

    private async Task<decimal> GetDecimalSettingAsync(string key, decimal fallback, CancellationToken cancellationToken)
    {
        var branchId = await GetCurrentBranchIdAsync(cancellationToken);
        if (branchId is null || branchId == Guid.Empty)
        {
            return fallback;
        }

        var setting = await _settingsRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Key == key, cancellationToken);

        return decimal.TryParse(setting?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    private async Task<Guid?> GetCurrentBranchIdAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedId))
        {
            return null;
        }

        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == parsedId, cancellationToken);

        return user?.DefaultBranchId;
    }
}
