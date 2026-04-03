using Microsoft.EntityFrameworkCore;
using ToptanciECommerce.Application.Interfaces.Repositories;
using ToptanciECommerce.Domain.Entities;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Infrastructure.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Product?> GetBySlugAsync(string slug) =>
        await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<Product?> GetWithImagesAsync(int id) =>
        await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId) =>
        await _dbSet
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .Include(p => p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder))
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 8) =>
        await _dbSet
            .Where(p => p.IsFeatured && p.IsActive)
            .Include(p => p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder))
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        int? categoryId = null,
        string? searchTerm = null,
        string? sortBy = null)
    {
        var query = _dbSet.Where(p => p.IsActive)
            .Include(p => p.Category)
            .Include(p => p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder))
            .AsQueryable();

        if (categoryId.HasValue)
        {
            // Include sub-category products when a parent category is selected
            var allCategoryIds = await _context.Set<Category>()
                .Where(c => c.Id == categoryId.Value || c.ParentCategoryId == categoryId.Value)
                .Select(c => c.Id)
                .ToListAsync();
            query = query.Where(p => allCategoryIds.Contains(p.CategoryId));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.SKU.Contains(searchTerm) ||
                (p.ShortDescription != null && p.ShortDescription.Contains(searchTerm)));

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.WholesalePrice),
            "price_desc" => query.OrderByDescending(p => p.WholesalePrice),
            "name_asc" => query.OrderBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> SkuExistsAsync(string sku, int? excludeId = null)
    {
        var query = _dbSet.Where(p => p.SKU == sku);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }

    public async Task UpdateStockAsync(int productId, int quantity)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product != null)
        {
            product.StockQuantity = quantity;
            product.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task AddProductImageAsync(ProductImage image) =>
        await _context.Set<ProductImage>().AddAsync(image);

    public async Task DeleteProductImageAsync(int imageId)
    {
        var img = await _context.Set<ProductImage>().FindAsync(imageId);
        if (img != null) _context.Set<ProductImage>().Remove(img);
    }
}
