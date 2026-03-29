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
}
