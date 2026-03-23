using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantManagement.Domain.Entities;
using RestaurantManagement.Infrastructure.Identity;
using RestaurantManagement.Infrastructure.Persistence;
using System.Data.Common;

namespace RestaurantManagement.Infrastructure.Seeding;

public sealed class AppDbInitializer
{
    private readonly AppDbContext _dbContext;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AppDbInitializer> _logger;

    public AppDbInitializer(
        AppDbContext dbContext,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AppDbInitializer> logger)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var provider = _dbContext.Database.ProviderName ?? "UnknownProvider";
        var connectionString = _dbContext.Database.GetConnectionString() ?? string.Empty;
        var (dataSource, database) = ParseConnectionTarget(connectionString);

        if (!OperatingSystem.IsWindows() && connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "DefaultConnection targets SQL LocalDB, which is not supported on Linux. " +
                $"Current target: Server='{dataSource}', Database='{database}'. " +
                "Use a reachable SQL Server instance/container and update the connection string.");
        }

        _logger.LogInformation(
            "Starting database initialization. Provider={Provider}, Server={Server}, Database={Database}",
            provider,
            dataSource,
            database);

        try
        {
            await _dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Database initialization failed during startup. " +
                $"Provider='{provider}', Server='{dataSource}', Database='{database}'. " +
                "Ensure SQL Server is running and reachable from this machine, then retry.",
                ex);
        }

        if (!await _dbContext.Branches.AnyAsync())
        {
            _dbContext.Branches.Add(new Branch
            {
                NameEn = "Main Branch",
                NameAr = "الفرع الرئيسي",
                Address = "Default Address"
            });

            await _dbContext.SaveChangesAsync();
        }

        await IdentitySeeder.SeedRolesAndAdminAsync(_roleManager, _userManager);
    }

    private static (string Server, string Database) ParseConnectionTarget(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return ("<empty>", "<empty>");
        }

        try
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            return (GetValue(builder, "Server", "Data Source"), GetValue(builder, "Database", "Initial Catalog"));
        }
        catch
        {
            return ("<unparsed>", "<unparsed>");
        }
    }

    private static string GetValue(DbConnectionStringBuilder builder, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (builder.TryGetValue(key, out var value) && value is not null)
            {
                return value.ToString() ?? "<null>";
            }
        }

        return "<not-set>";
    }
}
