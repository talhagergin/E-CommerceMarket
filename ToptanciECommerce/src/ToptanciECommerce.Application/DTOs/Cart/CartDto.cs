namespace ToptanciECommerce.Application.DTOs.Cart;

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal SubTotal => Items.Sum(x => x.LineTotal);
    public decimal TaxAmount => Items.Sum(x => x.TaxAmount);
    public decimal Total => SubTotal + TaxAmount;
    public int TotalQuantity => Items.Sum(x => x.Quantity);
}

public class CartItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public int Quantity { get; set; }
    public int QuantityStep { get; set; }
    public int MinOrderQuantity { get; set; }
    public int StockQuantity { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public decimal TaxAmount => LineTotal * (TaxRate / 100);
}

public class UpdateCartItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
