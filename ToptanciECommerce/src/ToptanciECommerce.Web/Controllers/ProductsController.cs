using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.Interfaces;

namespace ToptanciECommerce.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IUnitOfWork _uow;

    public ProductsController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // GET: /Products
    public async Task<IActionResult> Index(
        int? categoryId = null,
        string? search = null,
        string? sortBy = null,
        int page = 1)
    {
        const int pageSize = 24;

        var categories = await _uow.Categories.GetCategoriesWithSubsAsync();
        var (products, total) = await _uow.Products.GetPagedAsync(
            page, pageSize, categoryId, search, sortBy);

        // Resolve selected category name for breadcrumb
        string? selectedCategoryName = null;
        if (categoryId.HasValue)
        {
            var flat = categories.SelectMany(c =>
                new[] { c }.Concat(c.SubCategories)).ToList();
            selectedCategoryName = flat.FirstOrDefault(c => c.Id == categoryId.Value)?.Name;
        }

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.SelectedCategoryName = selectedCategoryName;
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = total;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);

        return View(products);
    }

    // GET: /Products/Detail/{slug}
    public async Task<IActionResult> Detail(string slug)
    {
        var product = await _uow.Products.GetBySlugAsync(slug);
        if (product == null) return NotFound();

        // Related products (same category, exclude current)
        var related = (await _uow.Products.GetByCategoryAsync(product.CategoryId))
            .Where(p => p.Id != product.Id)
            .Take(4)
            .ToList();

        ViewBag.RelatedProducts = related;
        return View(product);
    }
}
