using System.ComponentModel.DataAnnotations;

namespace ToptanciECommerce.Application.DTOs.Order;

public class ShippingInfoDto
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Firma adı zorunludur.")]
    public string CustomerCompany { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "İlçe zorunludur.")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şehir zorunludur.")]
    public string City { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
