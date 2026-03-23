using Microsoft.AspNetCore.Identity;

namespace RestaurantManagement.Infrastructure.Identity;

public sealed class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
