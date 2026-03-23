using RestaurantManagement.Domain.Common;

namespace RestaurantManagement.Domain.Entities;

public sealed class Category : SoftDeletableAuditableEntity
{
    public Guid BranchId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<Product> Products { get; set; } = new HashSet<Product>();
}
