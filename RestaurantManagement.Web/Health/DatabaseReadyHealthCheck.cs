using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RestaurantManagement.Infrastructure.Persistence;

namespace RestaurantManagement.Web.Health;

public sealed class DatabaseReadyHealthCheck : IHealthCheck
{
    private readonly AppDbContext _dbContext;

    public DatabaseReadyHealthCheck(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection is ready.")
                : HealthCheckResult.Unhealthy("Database connection is not ready.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database readiness check failed.", ex);
        }
    }
}
