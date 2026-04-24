using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities;

namespace YCT.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ImageUrl).HasMaxLength(500);

        // Info producto
        builder.Property(p => p.Weight).HasMaxLength(50);
        builder.Property(p => p.Ingredients).HasMaxLength(2000);
        builder.Property(p => p.StorageInstructions).HasMaxLength(500);
        builder.Property(p => p.ExpirationInfo).HasMaxLength(200);
        builder.Property(p => p.Brand).HasMaxLength(100);
        builder.Property(p => p.ServingSize).HasMaxLength(50);

        // Nutricional - precision para valores como 3.5g
        builder.Property(p => p.Calories).HasPrecision(8, 2);
        builder.Property(p => p.TotalFat).HasPrecision(8, 2);
        builder.Property(p => p.SaturatedFat).HasPrecision(8, 2);
        builder.Property(p => p.Cholesterol).HasPrecision(8, 2);
        builder.Property(p => p.Sodium).HasPrecision(8, 2);
        builder.Property(p => p.TotalCarbs).HasPrecision(8, 2);
        builder.Property(p => p.Sugars).HasPrecision(8, 2);
        builder.Property(p => p.Protein).HasPrecision(8, 2);
        builder.Property(p => p.Calcium).HasPrecision(8, 2);
        builder.Property(p => p.Iron).HasPrecision(8, 2);
        builder.Property(p => p.VitaminD).HasPrecision(8, 2);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
