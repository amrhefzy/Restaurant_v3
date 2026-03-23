using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class AppSetting : SoftDeletableAuditableEntity
{
    public Guid? BranchId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Branch? Branch { get; set; }
}
