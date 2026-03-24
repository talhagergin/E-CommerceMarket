using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetWithImagesAsync(int id);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(int categoryId);
    Task<IReadOnlyList<Product>> GetFeaturedAsync(int count = 8);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        int? categoryId = null,
        string? searchTerm = null,
        string? sortBy = null);
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);
    Task UpdateStockAsync(int productId, int quantity);
    /// <summary>Directly adds a ProductImage to the context (bypasses navigation-proxy issue).</summary>
    Task AddProductImageAsync(ProductImage image);
    Task DeleteProductImageAsync(int imageId);
}
