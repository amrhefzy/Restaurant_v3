using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Configuration;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.Localization;
using RestaurantManagement.Web.ViewModels.Settings;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class SettingsController : BranchScopedController
{
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
        await UpsertSettingAsync(settings, branchId, SettingsCatalog.Keys.CurrencyCode, model.CurrencyCode, _localizer[SettingsCatalog.Localization.DisplayCurrencyCode].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingsCatalog.Keys.TaxRatePercent, model.TaxRatePercent.ToString(CultureInfo.InvariantCulture), _localizer[SettingsCatalog.Localization.SalesTaxPercent].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingsCatalog.Keys.ServiceChargePercent, model.ServiceChargePercent.ToString(CultureInfo.InvariantCulture), _localizer[SettingsCatalog.Localization.ServiceChargePercentSetting].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingsCatalog.Keys.TrackInventory, model.TrackInventory.ToString(), _localizer[SettingsCatalog.Localization.TrackInventoryDescription].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingsCatalog.Keys.RequireLoginForOperations, model.RequireLoginForOperations.ToString(), _localizer[SettingsCatalog.Localization.RequireLoginForOperationsDescription].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingsCatalog.Keys.RoleAwareNavigationEnabled, model.RoleAwareNavigationEnabled.ToString(), _localizer[SettingsCatalog.Localization.RoleAwareNavigationDescription].Value, cancellationToken);
        await UpsertSettingAsync(settings, branchId, SettingsCatalog.Keys.RuntimeNotes, model.RuntimeNotes ?? string.Empty, _localizer[SettingsCatalog.Localization.RuntimeAndDeploymentNotesDescription].Value, cancellationToken);

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
            CurrencyCode = Get(SettingsCatalog.Keys.CurrencyCode, SettingsCatalog.Defaults.CurrencyCode),
            TaxRatePercent = GetDecimal(SettingsCatalog.Keys.TaxRatePercent, SettingsCatalog.Defaults.TaxRatePercent),
            ServiceChargePercent = GetDecimal(SettingsCatalog.Keys.ServiceChargePercent, SettingsCatalog.Defaults.ServiceChargePercent),
            TrackInventory = GetBool(SettingsCatalog.Keys.TrackInventory, SettingsCatalog.Defaults.TrackInventory),
            RequireLoginForOperations = GetBool(SettingsCatalog.Keys.RequireLoginForOperations, SettingsCatalog.Defaults.RequireLoginForOperations),
            RoleAwareNavigationEnabled = GetBool(SettingsCatalog.Keys.RoleAwareNavigationEnabled, SettingsCatalog.Defaults.RoleAwareNavigationEnabled),
            RuntimeNotes = Get(SettingsCatalog.Keys.RuntimeNotes, _localizer["DefaultRuntimeNotes"].Value)
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
