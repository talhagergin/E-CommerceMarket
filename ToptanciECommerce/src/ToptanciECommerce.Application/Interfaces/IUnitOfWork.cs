using ToptanciECommerce.Application.Interfaces.Repositories;

namespace ToptanciECommerce.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
