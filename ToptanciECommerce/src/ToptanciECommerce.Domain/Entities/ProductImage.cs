using ToptanciECommerce.Domain.Common;

namespace ToptanciECommerce.Domain.Entities;

public class ProductImage : BaseEntity
{
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Alt text for accessibility / SEO</summary>
    public string? AltText { get; set; }

    public bool IsMain { get; set; } = false;
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
