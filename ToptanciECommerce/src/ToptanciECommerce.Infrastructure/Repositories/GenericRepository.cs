using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ToptanciECommerce.Application.Interfaces.Repositories;
using ToptanciECommerce.Domain.Common;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public virtual async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.FirstOrDefaultAsync(predicate);

    public virtual async Task AddAsync(T entity) =>
        await _dbSet.AddAsync(entity);

    public virtual void Update(T entity) =>
        _dbSet.Update(entity);

    public virtual void Remove(T entity) =>
        _dbSet.Remove(entity);

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate == null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);
}
