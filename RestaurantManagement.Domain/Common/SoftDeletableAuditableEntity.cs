namespace RestaurantManagement.Domain.Common;

public abstract class SoftDeletableAuditableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOnUtc { get; set; }
    public string? DeletedBy { get; set; }
}
