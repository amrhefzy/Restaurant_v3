namespace RestaurantManagement.Web.Configuration;

public static class SettingsCatalog
{
    public static class Keys
    {
        public const string CurrencyCode = nameof(CurrencyCode);
        public const string TaxRatePercent = nameof(TaxRatePercent);
        public const string ServiceChargePercent = nameof(ServiceChargePercent);
        public const string TrackInventory = nameof(TrackInventory);
        public const string RequireLoginForOperations = nameof(RequireLoginForOperations);
        public const string RoleAwareNavigationEnabled = nameof(RoleAwareNavigationEnabled);
        public const string RuntimeNotes = nameof(RuntimeNotes);
    }

    public static class Defaults
    {
        public const string CurrencyCode = "EGP";
        public const decimal TaxRatePercent = 14m;
        public const decimal ServiceChargePercent = 0m;
        public const bool TrackInventory = true;
        public const bool RequireLoginForOperations = true;
        public const bool RoleAwareNavigationEnabled = true;
    }

    public static class Localization
    {
        public const string DisplayCurrencyCode = nameof(DisplayCurrencyCode);
        public const string SalesTaxPercent = nameof(SalesTaxPercent);
        public const string ServiceChargePercentSetting = nameof(ServiceChargePercentSetting);
        public const string TrackInventoryDescription = nameof(TrackInventoryDescription);
        public const string RequireLoginForOperationsDescription = nameof(RequireLoginForOperationsDescription);
        public const string RoleAwareNavigationDescription = nameof(RoleAwareNavigationDescription);
        public const string RuntimeAndDeploymentNotesDescription = nameof(RuntimeAndDeploymentNotesDescription);
    }

    public static class BlockedFlow
    {
        public sealed record Metadata(string SettingKey, string HintText, string ActionText, string ActionController, string ActionName);

        public static readonly Metadata InventoryTracking = new(
            Keys.TrackInventory,
            "Inventory-sensitive workflows are disabled at runtime for this branch.",
            "Open settings",
            "Settings",
            "Index");

        public static readonly Metadata OperationalLogin = new(
            Keys.RequireLoginForOperations,
            "Operational entry points require an authenticated session under the current branch settings.",
            "Sign in",
            "Account",
            "Login");
    }
}
