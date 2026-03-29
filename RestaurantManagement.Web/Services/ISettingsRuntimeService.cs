namespace RestaurantManagement.Web.Services;

public interface ISettingsRuntimeService
{
    Task<bool> IsRoleAwareNavigationEnabledAsync(CancellationToken cancellationToken = default);
    Task<bool> IsLoginRequiredForOperationsAsync(CancellationToken cancellationToken = default);
    Task<string> GetCurrencyCodeAsync(CancellationToken cancellationToken = default);
}
