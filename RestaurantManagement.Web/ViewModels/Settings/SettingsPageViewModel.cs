using System.ComponentModel.DataAnnotations;

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
    public string CurrencyCode { get; set; } = "EGP";

    [Range(0, 100)]
    public decimal TaxRatePercent { get; set; } = 14m;

    [Range(0, 100)]
    public decimal ServiceChargePercent { get; set; } = 0m;

    public bool TrackInventory { get; set; } = true;
    public bool RequireLoginForOperations { get; set; } = true;
    public bool RoleAwareNavigationEnabled { get; set; } = true;
    public string RuntimeNotes { get; set; } = string.Empty;
}
