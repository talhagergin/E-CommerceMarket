using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice).HasConversion<double>();
        builder.Property(x => x.TaxRate).HasConversion<double>();
        builder.Property(x => x.TaxAmount).HasConversion<double>();
        builder.Property(x => x.LineTotal).HasConversion<double>();
        builder.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProductSKU).IsRequired().HasMaxLength(50);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
