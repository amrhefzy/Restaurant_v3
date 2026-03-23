using RestaurantManagement.Application.DTOs.Dashboard;

namespace RestaurantManagement.Web.ViewModels.Dashboard;

public sealed class DashboardPageViewModel
{
    public DateTime BusinessDateUtc { get; set; }
    public DashboardSummaryDto Summary { get; set; } = new();
}
