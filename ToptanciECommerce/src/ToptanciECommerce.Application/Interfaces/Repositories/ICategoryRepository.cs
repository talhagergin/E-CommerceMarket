using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Application.Interfaces.Repositories;

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync();
    Task<IReadOnlyList<Category>> GetCategoriesWithSubsAsync();
    Task<Category?> GetBySlugAsync(string slug);
    Task<Category?> GetWithProductsAsync(int id);
}
