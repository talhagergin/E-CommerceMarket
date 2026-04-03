namespace ToptanciECommerce.Application.DTOs.Product;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal Price { get; set; }
    public decimal WholesalePrice { get; set; }
    public int MinOrderQuantity { get; set; }
    public int QuantityStep { get; set; }
    public int StockQuantity { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? MainImageUrl { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
}
