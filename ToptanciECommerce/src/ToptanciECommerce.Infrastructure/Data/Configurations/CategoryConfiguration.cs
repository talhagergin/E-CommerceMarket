using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Infrastructure.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);

        builder.HasIndex(x => x.Slug).IsUnique();

        // Self-referencing hierarchy
        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global query filter for soft delete
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
