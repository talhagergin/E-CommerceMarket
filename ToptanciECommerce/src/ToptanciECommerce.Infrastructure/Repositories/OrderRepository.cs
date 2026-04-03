using Microsoft.EntityFrameworkCore;
using ToptanciECommerce.Application.Interfaces.Repositories;
using ToptanciECommerce.Domain.Entities;
using ToptanciECommerce.Domain.Enums;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Infrastructure.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Order?> GetWithItemsAsync(int id) =>
        await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber) =>
        await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

    public async Task<Order?> GetByIyzicoTokenAsync(string token) =>
        await _dbSet.FirstOrDefaultAsync(o => o.IyzicoToken == token);

    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(string customerId) =>
        await _dbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        OrderStatus? status = null,
        string? searchTerm = null)
    {
        var query = _dbSet.AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(o =>
                o.OrderNumber.Contains(searchTerm) ||
                o.CustomerName.Contains(searchTerm) ||
                o.CustomerEmail.Contains(searchTerm) ||
                o.CustomerCompany.Contains(searchTerm));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _dbSet.FindAsync(orderId);
        if (order != null)
        {
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
        }
    }
}
