using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ToptanciECommerce.Application.DTOs.Category;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int? ParentCategoryId { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public IFormFile? Image { get; set; }
}
