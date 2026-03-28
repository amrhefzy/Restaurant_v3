using Microsoft.AspNetCore.Identity;
using RestaurantManagement.Domain.Constants;
using RestaurantManagement.Infrastructure.Identity;

namespace RestaurantManagement.Infrastructure.Seeding;

public static class IdentitySeeder
{
    public static async Task SeedRolesAndAdminAsync(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        foreach (var role in SystemRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = role,
                NormalizedName = role.ToUpperInvariant(),
                Description = $"{role} role"
            });
        }

        const string adminEmail = "admin@gmail";
        const string adminPassword = "Admin@123456";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is not null)
        {
            return;
        }

        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Super Admin",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, SystemRoles.SuperAdmin);
        }
    }
}
