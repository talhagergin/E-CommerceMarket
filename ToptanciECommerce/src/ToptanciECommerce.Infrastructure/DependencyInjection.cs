using Microsoft.AspNetCore.DataProtection;
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

        // Persist DP keys in PostgreSQL so session/auth/antiforgery cookies survive deploys (Render, Docker).
        services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>()
            .SetApplicationName("ToptanciECommerce");

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Use R2 cloud storage if configured, otherwise fall back to local filesystem
        var r2Endpoint = configuration["CloudflareR2:Endpoint"];
        if (!string.IsNullOrWhiteSpace(r2Endpoint))
            services.AddScoped<IImageService, R2ImageService>();
        else
            services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IOrderPdfService, OrderPdfService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICartService, CartService>();

        return services;
    }
}
