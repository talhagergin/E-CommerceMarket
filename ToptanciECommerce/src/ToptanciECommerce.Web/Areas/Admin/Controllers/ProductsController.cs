using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ToptanciECommerce.Application.DTOs.Product;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Services;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IImageService _imageService;

    public ProductsController(IUnitOfWork uow, IImageService imageService)
    {
        _uow = uow;
        _imageService = imageService;
    }

    // GET: Admin/Products
    public async Task<IActionResult> Index(int page = 1, string? search = null, int? categoryId = null)
    {
        const int pageSize = 20;
        var (products, total) = await _uow.Products.GetPagedAsync(page, pageSize, categoryId, search);

        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.Search = search;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.Categories = new SelectList(await _uow.Categories.GetAllAsync(), "Id", "Name", categoryId);

        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Price = p.Price,
            WholesalePrice = p.WholesalePrice,
            StockQuantity = p.StockQuantity,
            IsActive = p.IsActive,
            IsFeatured = p.IsFeatured,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? "",
            MainImageUrl = p.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                         ?? p.Images.FirstOrDefault()?.ImageUrl
        }).ToList();

        return View(dtos);
    }

    // GET: Admin/Products/Create
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new CreateEditProductDto());
    }

    // POST: Admin/Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEditProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync();
            return View(dto);
        }

        if (await _uow.Products.SkuExistsAsync(dto.SKU))
        {
            ModelState.AddModelError(nameof(dto.SKU), "Bu SKU zaten kullanılmaktadır.");
            await PopulateCategoriesAsync();
            return View(dto);
        }

        var slug = GenerateSlug(dto.Name);
        var product = new Product
        {
            Name = dto.Name,
            Slug = slug,
            SKU = dto.SKU,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            Price = dto.Price,
            WholesalePrice = dto.WholesalePrice,
            MinOrderQuantity = dto.MinOrderQuantity,
            QuantityStep = dto.QuantityStep,
            StockQuantity = dto.StockQuantity,
            TaxRate = dto.TaxRate,
            IsActive = dto.IsActive,
            IsFeatured = dto.IsFeatured,
            CategoryId = dto.CategoryId
        };

        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync(); // product.Id is now assigned

        // Handle image uploads — use AddProductImageAsync to bypass EF tracking proxy issue
        if (dto.NewImages != null && dto.NewImages.Count > 0)
        {
            int displayOrder = 0;
            bool firstImage = true;
            foreach (var file in dto.NewImages)
            {
                var url = await _imageService.SaveImageAsync(file, "products");
                await _uow.Products.AddProductImageAsync(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    IsMain = firstImage,
                    DisplayOrder = displayOrder++
                });
                firstImage = false;
            }
            await _uow.SaveChangesAsync();
        }

        TempData["Success"] = $"'{product.Name}' ürünü başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Products/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _uow.Products.GetWithImagesAsync(id);
        if (product == null) return NotFound();

        await PopulateCategoriesAsync(product.CategoryId);

        var dto = new CreateEditProductDto
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            Price = product.Price,
            WholesalePrice = product.WholesalePrice,
            MinOrderQuantity = product.MinOrderQuantity,
            QuantityStep = product.QuantityStep,
            StockQuantity = product.StockQuantity,
            TaxRate = product.TaxRate,
            IsActive = product.IsActive,
            IsFeatured = product.IsFeatured,
            CategoryId = product.CategoryId,
            MainImageId = product.Images.FirstOrDefault(i => i.IsMain)?.Id
        };

        ViewBag.ExistingImages = product.Images.OrderBy(i => i.DisplayOrder).ToList();
        return View(dto);
    }

    // POST: Admin/Products/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateEditProductDto dto)
    {
        if (id != dto.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(dto.CategoryId);
            var product2 = await _uow.Products.GetWithImagesAsync(id);
            ViewBag.ExistingImages = product2?.Images.OrderBy(i => i.DisplayOrder).ToList();
            return View(dto);
        }

        var product = await _uow.Products.GetWithImagesAsync(id);
        if (product == null) return NotFound();

        if (await _uow.Products.SkuExistsAsync(dto.SKU, id))
        {
            ModelState.AddModelError(nameof(dto.SKU), "Bu SKU zaten kullanılmaktadır.");
            await PopulateCategoriesAsync(dto.CategoryId);
            ViewBag.ExistingImages = product.Images.OrderBy(i => i.DisplayOrder).ToList();
            return View(dto);
        }

        product.Name = dto.Name;
        product.SKU = dto.SKU;
        product.Description = dto.Description;
        product.ShortDescription = dto.ShortDescription;
        product.Price = dto.Price;
        product.WholesalePrice = dto.WholesalePrice;
        product.MinOrderQuantity = dto.MinOrderQuantity;
        product.QuantityStep = dto.QuantityStep;
        product.StockQuantity = dto.StockQuantity;
        product.TaxRate = dto.TaxRate;
        product.IsActive = dto.IsActive;
        product.IsFeatured = dto.IsFeatured;
        product.CategoryId = dto.CategoryId;

        // Delete requested images
        if (dto.DeleteImageIds != null)
        {
            foreach (var imgId in dto.DeleteImageIds)
            {
                var img = product.Images.FirstOrDefault(i => i.Id == imgId);
                if (img != null)
                {
                    await _imageService.DeleteImageAsync(img.ImageUrl);
                    await _uow.Products.DeleteProductImageAsync(imgId);
                }
            }
        }

        // Set main image
        if (dto.MainImageId.HasValue)
        {
            foreach (var img in product.Images)
                img.IsMain = img.Id == dto.MainImageId.Value;
        }

        // Upload new images
        if (dto.NewImages != null)
        {
            int nextOrder = product.Images.Count;
            bool hasMain = product.Images.Any(i => i.IsMain);
            foreach (var file in dto.NewImages)
            {
                var url = await _imageService.SaveImageAsync(file, "products");
                await _uow.Products.AddProductImageAsync(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = url,
                    IsMain = !hasMain,
                    DisplayOrder = nextOrder++
                });
                hasMain = true;
            }
        }

        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();

        TempData["Success"] = $"'{product.Name}' ürünü başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Products/StockPriceUpdate
    public async Task<IActionResult> StockPriceUpdate(int? categoryId)
    {
        var products = categoryId.HasValue
            ? await _uow.Products.GetByCategoryAsync(categoryId.Value)
            : await _uow.Products.GetAllAsync();

        ViewBag.Categories = new SelectList(await _uow.Categories.GetAllAsync(), "Id", "Name", categoryId);
        ViewBag.SelectedCategoryId = categoryId;

        var dtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Price = p.Price,
            WholesalePrice = p.WholesalePrice,
            StockQuantity = p.StockQuantity,
            TaxRate = p.TaxRate,
            IsActive = p.IsActive
        }).ToList();

        return View(dtos);
    }

    // POST: Admin/Products/BulkUpdatePricesStock
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpdatePricesStock(List<ProductDto> updates)
    {
        foreach (var update in updates)
        {
            var product = await _uow.Products.GetByIdAsync(update.Id);
            if (product == null) continue;
            product.Price = update.Price;
            product.WholesalePrice = update.WholesalePrice;
            product.StockQuantity = update.StockQuantity;
            _uow.Products.Update(product);
        }

        await _uow.SaveChangesAsync();
        TempData["Success"] = "Fiyat ve stok bilgileri güncellendi.";
        return RedirectToAction(nameof(StockPriceUpdate));
    }

    // POST: Admin/Products/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _uow.Products.GetWithImagesAsync(id);
        if (product == null) return NotFound();

        // Soft delete
        product.IsDeleted = true;
        _uow.Products.Update(product);
        await _uow.SaveChangesAsync();

        TempData["Success"] = $"'{product.Name}' ürünü silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        var categories = await _uow.Categories.GetAllAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedId);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug;
    }
}
