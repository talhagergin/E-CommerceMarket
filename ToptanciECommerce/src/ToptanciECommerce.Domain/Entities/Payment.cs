using ToptanciECommerce.Domain.Common;

namespace ToptanciECommerce.Domain.Entities;

public class Payment : BaseEntity
{
    public string CustomerId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Nakit, Kart, Havale/EFT, Çek</summary>
    public string PaymentMethod { get; set; } = "Nakit";

    /// <summary>Tahsilat veya İade</summary>
    public string PaymentType { get; set; } = "Tahsilat";

    public string? Notes { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.Now;

    /// <summary>Opsiyonel olarak belirli bir siparişe bağlanabilir</summary>
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public string? RecordedByUserId { get; set; }
}
