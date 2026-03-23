namespace RestaurantManagement.Application.DTOs.Categories;

public sealed class UpdateCategoryDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
