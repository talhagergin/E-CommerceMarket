using ToptanciECommerce.Domain.Common;
using ToptanciECommerce.Domain.Enums;

namespace ToptanciECommerce.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;

    // Customer info (denormalized for history)
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerCompany { get; set; } = string.Empty;

    // Shipping address
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingDistrict { get; set; } = string.Empty;
    public string ShippingPostalCode { get; set; } = string.Empty;
    public string ShippingCountry { get; set; } = "Türkiye";

    // Amounts
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Payment
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? IyzicoPaymentId { get; set; }
    public string? IyzicoToken { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }

    /// <summary>Peşin, Kart, Veresiye</summary>
    public string SaleType { get; set; } = "Veresiye";

    // Navigation
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
