namespace ToptanciECommerce.Application.DTOs.Category;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public int? DisplayOrder { get; set; }
    public List<CategoryDto> SubCategories { get; set; } = new();
    public int ProductCount { get; set; }
}
