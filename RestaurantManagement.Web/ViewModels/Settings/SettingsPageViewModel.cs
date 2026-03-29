using System.ComponentModel.DataAnnotations;
using RestaurantManagement.Web.Configuration;

namespace RestaurantManagement.Web.ViewModels.Settings;

public sealed class SettingsPageViewModel
{
    public Guid? ActiveBranchId { get; set; }

    [Required]
    public string BranchNameEn { get; set; } = string.Empty;

    [Required]
    public string BranchNameAr { get; set; } = string.Empty;

    public string? BranchAddress { get; set; }
    public string? BranchPhone { get; set; }

    [Required]
    public string CurrencyCode { get; set; } = SettingsCatalog.Defaults.CurrencyCode;

    [Range(0, 100)]
    public decimal TaxRatePercent { get; set; } = SettingsCatalog.Defaults.TaxRatePercent;

    [Range(0, 100)]
    public decimal ServiceChargePercent { get; set; } = SettingsCatalog.Defaults.ServiceChargePercent;

    public bool TrackInventory { get; set; } = SettingsCatalog.Defaults.TrackInventory;
    public bool RequireLoginForOperations { get; set; } = SettingsCatalog.Defaults.RequireLoginForOperations;
    public bool RoleAwareNavigationEnabled { get; set; } = SettingsCatalog.Defaults.RoleAwareNavigationEnabled;
    public string RuntimeNotes { get; set; } = string.Empty;
}
