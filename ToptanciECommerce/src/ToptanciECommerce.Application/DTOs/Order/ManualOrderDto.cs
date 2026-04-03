using System.ComponentModel.DataAnnotations;

namespace ToptanciECommerce.Application.DTOs.Order;

public class ManualOrderDto
{
    [Required(ErrorMessage = "Müşteri seçimi zorunludur.")]
    public string CustomerId { get; set; } = string.Empty;

    public string? Notes { get; set; }
    public string? PaymentMethod { get; set; }
    /// <summary>Peşin, Kart, Veresiye</summary>
    public string? SaleType { get; set; } = "Veresiye";

    public List<ManualOrderLineDto> Lines { get; set; } = new();
}

public class ManualOrderLineDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, 99999)]
    public int Quantity { get; set; }
}
