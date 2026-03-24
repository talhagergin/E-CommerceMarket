using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ToptanciECommerce.Application.DTOs.Category;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Services;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IImageService _imageService;

    public CategoriesController(IUnitOfWork uow, IImageService imageService)
    {
        _uow = uow;
        _imageService = imageService;
    }

    // GET: Admin/Categories
    public async Task<IActionResult> Index()
    {
        var categories = await _uow.Categories.GetCategoriesWithSubsAsync();
        var dtos = categories.Select(c => MapToDto(c)).ToList();
        return View(dtos);
    }

    // GET: Admin/Categories/Create
    public async Task<IActionResult> Create()
    {
        await PopulateParentCategoriesAsync();
        return View(new CreateCategoryDto());
    }

    // POST: Admin/Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentCategoriesAsync();
            return View(dto);
        }

        var slug = GenerateSlug(dto.Name);

        string? imageUrl = null;
        if (dto.Image != null)
            imageUrl = await _imageService.SaveImageAsync(dto.Image, "categories");

        var category = new Category
        {
            Name = dto.Name,
            Slug = slug,
            Description = dto.Description,
            ImageUrl = imageUrl,
            ParentCategoryId = dto.ParentCategoryId,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();

        TempData["Success"] = $"'{category.Name}' kategorisi oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Categories/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null) return NotFound();

        await PopulateParentCategoriesAsync(category.ParentCategoryId, id);

        var dto = new CreateCategoryDto
        {
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };

        ViewBag.CategoryId = id;
        ViewBag.ExistingImageUrl = category.ImageUrl;
        return View(dto);
    }

    // POST: Admin/Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateParentCategoriesAsync(dto.ParentCategoryId, id);
            ViewBag.CategoryId = id;
            return View(dto);
        }

        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null) return NotFound();

        if (dto.Image != null)
        {
            // Delete old image
            if (!string.IsNullOrEmpty(category.ImageUrl))
                await _imageService.DeleteImageAsync(category.ImageUrl);

            category.ImageUrl = await _imageService.SaveImageAsync(dto.Image, "categories");
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.DisplayOrder = dto.DisplayOrder;
        category.IsActive = dto.IsActive;

        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();

        TempData["Success"] = $"'{category.Name}' kategorisi güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Admin/Categories/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category == null) return NotFound();

        category.IsDeleted = true;
        _uow.Categories.Update(category);
        await _uow.SaveChangesAsync();

        TempData["Success"] = $"'{category.Name}' kategorisi silindi.";
        return RedirectToAction(nameof(Index));
    }

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        ParentCategoryId = c.ParentCategoryId,
        IsActive = c.IsActive,
        DisplayOrder = c.DisplayOrder,
        SubCategories = c.SubCategories.Select(s => MapToDto(s)).ToList()
    };

    private async Task PopulateParentCategoriesAsync(int? selectedId = null, int? excludeId = null)
    {
        var all = await _uow.Categories.GetAllAsync();
        var filtered = excludeId.HasValue
            ? all.Where(c => c.Id != excludeId.Value).ToList()
            : all.ToList();
        ViewBag.ParentCategories = new SelectList(filtered, "Id", "Name", selectedId);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        return System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
    }
}
