using Microsoft.AspNetCore.Identity;

namespace RestaurantManagement.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public Guid? DefaultBranchId { get; set; }
}
