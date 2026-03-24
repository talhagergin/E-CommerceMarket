using ToptanciECommerce.Domain.Common;

namespace ToptanciECommerce.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly slug (e.g. "elektronik-urunler")</summary>
    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Self-referencing hierarchy
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
