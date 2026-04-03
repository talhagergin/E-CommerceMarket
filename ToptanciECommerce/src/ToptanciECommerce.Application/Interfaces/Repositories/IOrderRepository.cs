using ToptanciECommerce.Domain.Entities;
using ToptanciECommerce.Domain.Enums;

namespace ToptanciECommerce.Application.Interfaces.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetWithItemsAsync(int id);
    Task<Order?> GetByOrderNumberAsync(string orderNumber);
    Task<Order?> GetByIyzicoTokenAsync(string token);
    Task<IReadOnlyList<Order>> GetByCustomerAsync(string customerId);
    Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        OrderStatus? status = null,
        string? searchTerm = null);
    Task UpdateStatusAsync(int orderId, OrderStatus status);
}
