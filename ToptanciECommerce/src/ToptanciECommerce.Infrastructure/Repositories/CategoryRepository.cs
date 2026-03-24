using Microsoft.EntityFrameworkCore;
using ToptanciECommerce.Application.Interfaces.Repositories;
using ToptanciECommerce.Domain.Entities;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Infrastructure.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync() =>
        await _dbSet
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<Category>> GetCategoriesWithSubsAsync() =>
        await _dbSet
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .Include(c => c.SubCategories.Where(s => s.IsActive))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<Category?> GetBySlugAsync(string slug) =>
        await _dbSet
            .Include(c => c.SubCategories.Where(s => s.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<Category?> GetWithProductsAsync(int id) =>
        await _dbSet
            .Include(c => c.Products.Where(p => p.IsActive))
                .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == id);
}
