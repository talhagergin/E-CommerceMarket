using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Services;
using ToptanciECommerce.Infrastructure.Data;
using ToptanciECommerce.Infrastructure.Repositories;
using ToptanciECommerce.Infrastructure.Services;
using ToptanciECommerce.Application.Interfaces.Repositories;

namespace ToptanciECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IOrderPdfService, OrderPdfService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICartService, CartService>();

        return services;
    }
}
