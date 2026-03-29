namespace RestaurantManagement.Web.ViewModels.Settings;

public sealed class SettingsPageViewModel
{
    public Guid? ActiveBranchId { get; set; }
    public string CurrencyCode { get; set; } = "EGP";
    public decimal TaxRatePercent { get; set; } = 14m;
    public decimal ServiceChargePercent { get; set; } = 0m;
    public bool TrackInventory { get; set; } = true;
    public bool RequireLoginForOperations { get; set; } = true;
    public bool RoleAwareNavigationEnabled { get; set; } = true;
    public string RuntimeNotes { get; set; } = string.Empty;
}
