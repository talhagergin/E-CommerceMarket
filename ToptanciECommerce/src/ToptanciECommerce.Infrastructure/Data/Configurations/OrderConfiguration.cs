using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(30);
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.CustomerEmail).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CustomerPhone).HasMaxLength(20);
        builder.Property(x => x.CustomerCompany).HasMaxLength(200);
        builder.Property(x => x.ShippingAddress).HasMaxLength(500);
        builder.Property(x => x.SubTotal).HasConversion<double>();
        builder.Property(x => x.TaxAmount).HasConversion<double>();
        builder.Property(x => x.ShippingAmount).HasConversion<double>();
        builder.Property(x => x.DiscountAmount).HasConversion<double>();
        builder.Property(x => x.TotalAmount).HasConversion<double>();
        builder.Property(x => x.IyzicoPaymentId).HasMaxLength(100);
        builder.Property(x => x.IyzicoToken).HasMaxLength(500);

        builder.HasIndex(x => x.OrderNumber).IsUnique();
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.Status);
    }
}
