using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Application.Interfaces.Repositories;

public interface IPaymentRepository : IGenericRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByCustomerAsync(string customerId);
    Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<decimal> GetCustomerTotalPaidAsync(string customerId);
}
