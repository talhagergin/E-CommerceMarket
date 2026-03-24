using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ToptanciECommerce.Application.DTOs.Product;

public class CreateEditProductDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU zorunludur.")]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    public string? Description { get; set; }

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
    public decimal Price { get; set; }

    [Required, Range(0.01, double.MaxValue)]
    public decimal WholesalePrice { get; set; }

    [Range(1, int.MaxValue)]
    public int MinOrderQuantity { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int QuantityStep { get; set; } = 1;

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public decimal TaxRate { get; set; } = 18m;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;

    [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
    public int CategoryId { get; set; }

    /// <summary>Multiple images uploaded from form</summary>
    public List<IFormFile>? NewImages { get; set; }

    /// <summary>IDs of existing images to delete</summary>
    public List<int>? DeleteImageIds { get; set; }

    /// <summary>ID of the image to set as main</summary>
    public int? MainImageId { get; set; }
}
