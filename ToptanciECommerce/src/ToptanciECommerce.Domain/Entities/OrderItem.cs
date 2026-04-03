using ToptanciECommerce.Domain.Common;

namespace ToptanciECommerce.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int Quantity { get; set; }

    /// <summary>Unit price at order time (snapshot)</summary>
    public decimal UnitPrice { get; set; }

    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    // Snapshot fields for history
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;

    // Navigation
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
