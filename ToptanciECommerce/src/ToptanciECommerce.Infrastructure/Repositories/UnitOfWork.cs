using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Repositories;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Categories = new CategoryRepository(context);
        Products   = new ProductRepository(context);
        Orders     = new OrderRepository(context);
        Payments   = new PaymentRepository(context);
    }

    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public IOrderRepository Orders { get; }
    public IPaymentRepository Payments { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
