using ToptanciECommerce.Domain.Common;

namespace ToptanciECommerce.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    /// <summary>Stock Keeping Unit</summary>
    public string SKU { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ShortDescription { get; set; }

    /// <summary>Retail / listed price (KDV dahil)</summary>
    public decimal Price { get; set; }

    /// <summary>B2B toptancı fiyatı</summary>
    public decimal WholesalePrice { get; set; }

    /// <summary>Minimum sipariş adedi</summary>
    public int MinOrderQuantity { get; set; } = 1;

    /// <summary>Adım miktarı (örn. 6'lı koli için 6)</summary>
    public int QuantityStep { get; set; } = 1;

    public int StockQuantity { get; set; }
    public bool TrackInventory { get; set; } = true;
    public decimal? Weight { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public decimal TaxRate { get; set; } = 18m; // KDV %

    // Navigation
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
