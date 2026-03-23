using System.Globalization;
using RestaurantManagement.Application.Common.Interfaces;

namespace RestaurantManagement.Web.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value
                             ?? _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;

    public string? UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public string? CurrentCulture => CultureInfo.CurrentUICulture.Name;
}
