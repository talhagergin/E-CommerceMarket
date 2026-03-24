using Microsoft.EntityFrameworkCore;
using ToptanciECommerce.Application.Interfaces.Repositories;
using ToptanciECommerce.Domain.Entities;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Infrastructure.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Payment>> GetByCustomerAsync(string customerId) =>
        await _dbSet
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<IReadOnlyList<Payment>> GetByDateRangeAsync(DateTime from, DateTime to) =>
        await _dbSet
            .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<decimal> GetCustomerTotalPaidAsync(string customerId) =>
        await _dbSet
            .Where(p => p.CustomerId == customerId && p.PaymentType == "Tahsilat")
            .SumAsync(p => p.Amount);
}
