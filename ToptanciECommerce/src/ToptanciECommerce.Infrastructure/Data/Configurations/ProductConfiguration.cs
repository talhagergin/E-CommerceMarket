using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(220);
        builder.Property(x => x.SKU).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description);
        builder.Property(x => x.ShortDescription).HasMaxLength(500);
        builder.Property(x => x.Price).HasConversion<double>();
        builder.Property(x => x.WholesalePrice).HasConversion<double>();
        builder.Property(x => x.TaxRate).HasConversion<double>();
        builder.Property(x => x.Weight).HasConversion<double>();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.SKU).IsUnique();
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.IsActive);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
