namespace RestaurantManagement.Domain.Constants;

public static class SystemRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";

    public static IReadOnlyCollection<string> All => [SuperAdmin, Manager, Cashier];
}
