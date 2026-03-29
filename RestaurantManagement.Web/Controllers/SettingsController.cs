using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Application.Common.Interfaces;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Web.Controllers.Base;
using RestaurantManagement.Web.ViewModels.Settings;

namespace RestaurantManagement.Web.Controllers;

[Authorize(Roles = "SuperAdmin,Manager")]
public sealed class SettingsController : BranchScopedController
{
    private readonly IRepository<Branch> _branchRepository;
    private readonly IRepository<AppSetting> _settingsRepository;

    public SettingsController(
        IRepository<Branch> branchRepository,
        IRepository<AppSetting> settingsRepository) : base(branchRepository)
    {
        _branchRepository = branchRepository;
        _settingsRepository = settingsRepository;
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
            TempData["Error"] = "No branch is configured.";
            return RedirectToAction(nameof(Index));
        }

        branch.NameEn = model.BranchNameEn;
        branch.NameAr = model.BranchNameAr;
        branch.Address = model.BranchAddress;
        branch.Phone = model.BranchPhone;
        _branchRepository.Update(branch);

        var settings = await _settingsRepository.ListAsync(x => x.BranchId == branchId, cancellationToken);
        await UpsertSettingAsync(settings, branchId, "CurrencyCode", model.CurrencyCode, "Display currency code", cancellationToken);
        await UpsertSettingAsync(settings, branchId, "TaxRatePercent", model.TaxRatePercent.ToString(System.Globalization.CultureInfo.InvariantCulture), "Sales tax percent", cancellationToken);
        await UpsertSettingAsync(settings, branchId, "ServiceChargePercent", model.ServiceChargePercent.ToString(System.Globalization.CultureInfo.InvariantCulture), "Service charge percent", cancellationToken);
        await UpsertSettingAsync(settings, branchId, "TrackInventory", model.TrackInventory.ToString(), "Whether inventory tracking is enabled", cancellationToken);
        await UpsertSettingAsync(settings, branchId, "RequireLoginForOperations", model.RequireLoginForOperations.ToString(), "Whether login is required for operations", cancellationToken);
        await UpsertSettingAsync(settings, branchId, "RoleAwareNavigationEnabled", model.RoleAwareNavigationEnabled.ToString(), "Whether role-aware navigation is enabled", cancellationToken);
        await UpsertSettingAsync(settings, branchId, "RuntimeNotes", model.RuntimeNotes ?? string.Empty, "Runtime and deployment notes", cancellationToken);

        TempData["Success"] = "Settings saved successfully.";
        return RedirectToAction(nameof(Index));
    }

    private SettingsPageViewModel BuildViewModel(Branch? branch, IReadOnlyList<AppSetting> settings)
    {
        string Get(string key, string fallback = "") => settings.FirstOrDefault(x => x.Key == key)?.Value ?? fallback;
        bool GetBool(string key, bool fallback) => bool.TryParse(Get(key), out var value) ? value : fallback;
        decimal GetDecimal(string key, decimal fallback) => decimal.TryParse(Get(key, fallback.ToString(System.Globalization.CultureInfo.InvariantCulture)), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : fallback;

        return new SettingsPageViewModel
        {
            ActiveBranchId = branch?.Id,
            BranchNameEn = branch?.NameEn ?? string.Empty,
            BranchNameAr = branch?.NameAr ?? string.Empty,
            BranchAddress = branch?.Address,
            BranchPhone = branch?.Phone,
            CurrencyCode = Get("CurrencyCode", "EGP"),
            TaxRatePercent = GetDecimal("TaxRatePercent", 14m),
            ServiceChargePercent = GetDecimal("ServiceChargePercent", 0m),
            TrackInventory = GetBool("TrackInventory", true),
            RequireLoginForOperations = GetBool("RequireLoginForOperations", true),
            RoleAwareNavigationEnabled = GetBool("RoleAwareNavigationEnabled", true),
            RuntimeNotes = Get("RuntimeNotes", "Linux runtime active, SQL Server runs via Docker, auth and role-aware navigation are enabled.")
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
