using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.ViewModels.Settings;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class SettingsController : BranchScopedController
{
    private static class SettingKeys
    {
        public const string CurrencyCode = nameof(CurrencyCode);
        public const string TaxRatePercent = nameof(TaxRatePercent);
        public const string ServiceChargePercent = nameof(ServiceChargePercent);
        public const string TrackInventory = nameof(TrackInventory);
        public const string RequireLoginForOperations = nameof(RequireLoginForOperations);
        public const string RoleAwareNavigationEnabled = nameof(RoleAwareNavigationEnabled);
        public const string RuntimeNotes = nameof(RuntimeNotes);
    }

    private readonly IRepository<Branch> _branchRepository;
    private readonly IRepository<AppSetting> _settingsRepository;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public SettingsController(
        IRepository<Branch> branchRepository,
        IRepository<AppSetting> settingsRepository,
        IStringLocalizer<SharedResource> localizer) : base(branchRepository)
    {
        _branchRepository = branchRepository;
        _settingsRepository = settingsRepository;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var branchId = await GetBranchIdAsync(cancellationToken);
        var branch = branchId == Guid.Empty ? null : await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        var appSettings = branchId == Guid.Empty
            ? Array.Empty<AppSetting>()
            : await _settingsRepository.ListAsync(x => x.BranchId == branchId, cancellationToken);

        return View(BuildViewModel(branch, appSettings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SettingsPageViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var branchId = await GetBranchIdAsync(cancellationToken);
        var branch = branchId == Guid.Empty ? null : await _branchRepository.GetByIdAsync(branchId, cancellationToken);
        if (branch is null)
        {
            TempData["Error"] = _localizer["NoBranchConfigured"].Value;
            return RedirectToAction(nameof(Index));
        }

        branch.NameEn = model.BranchNameEn;
        branch.NameAr = model.BranchNameAr;
        branch.Address = model.BranchAddress;
        branch.Phone = model.BranchPhone;
        _branchRepository.Update(branch);

        var settings = await _settingsRepository.ListAsync(x => x.BranchId == branchId, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingKeys.CurrencyCode, model.CurrencyCode, _localizer["DisplayCurrencyCode"].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingKeys.TaxRatePercent, model.TaxRatePercent.ToString(CultureInfo.InvariantCulture), _localizer["SalesTaxPercent"].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingKeys.ServiceChargePercent, model.ServiceChargePercent.ToString(CultureInfo.InvariantCulture), _localizer["ServiceChargePercentSetting"].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingKeys.TrackInventory, model.TrackInventory.ToString(), _localizer["TrackInventoryDescription"].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingKeys.RequireLoginForOperations, model.RequireLoginForOperations.ToString(), _localizer["RequireLoginForOperationsDescription"].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingKeys.RoleAwareNavigationEnabled, model.RoleAwareNavigationEnabled.ToString(), _localizer["RoleAwareNavigationDescription"].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingKeys.RuntimeNotes, model.RuntimeNotes ?? string.Empty, _localizer["RuntimeAndDeploymentNotesDescription"].Value, cancellationToken);

        TempData["Success"] = _localizer["SettingsSavedSuccessfully"].Value;
        return RedirectToAction(nameof(Index));
    }

    private SettingsPageViewModel BuildViewModel(Branch? branch, IReadOnlyList<AppSetting> settings)
    {
        string Get(string key, string fallback = "") => settings.FirstOrDefault(x => x.Key == key)?.Value ?? fallback;
        bool GetBool(string key, bool fallback) => bool.TryParse(Get(key), out var value) ? value : fallback;
        decimal GetDecimal(string key, decimal fallback)
            => decimal.TryParse(Get(key, fallback.ToString(CultureInfo.InvariantCulture)), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;

        return new SettingsPageViewModel
        {
            ActiveBranchId = branch?.Id,
            BranchNameEn = branch?.NameEn ?? string.Empty,
            BranchNameAr = branch?.NameAr ?? string.Empty,
            BranchAddress = branch?.Address,
            BranchPhone = branch?.Phone,
            CurrencyCode = Get(SettingKeys.CurrencyCode, "EGP"),
            TaxRatePercent = GetDecimal(SettingKeys.TaxRatePercent, 14m),
            ServiceChargePercent = GetDecimal(SettingKeys.ServiceChargePercent, 0m),
            TrackInventory = GetBool(SettingKeys.TrackInventory, true),
            RequireLoginForOperations = GetBool(SettingKeys.RequireLoginForOperations, true),
            RoleAwareNavigationEnabled = GetBool(SettingKeys.RoleAwareNavigationEnabled, true),
            RuntimeNotes = Get(SettingKeys.RuntimeNotes, _localizer["DefaultRuntimeNotes"].Value)
        };
    }

    private async Task UpsertSettingAsync(IReadOnlyList<AppSetting> existingSettings, Guid branchId, string key, string value, string? description, CancellationToken cancellationToken)
    {
        var setting = existingSettings.FirstOrDefault(x => x.Key == key);
        if (setting is null)
        {
            await _settingsRepository.AddAsync(new AppSetting
            {
                Id = Guid.NewGuid(),
                BranchId = branchId,
                Key = key,
                Value = value,
                Description = description
            }, cancellationToken);
            return;
        }

        setting.Value = value;
        setting.Description = description;
        _settingsRepository.Update(setting);
    }
}
