using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities;

namespace YCT.Infrastructure.Persistence.Configurations;

public class DistributorConfiguration : IEntityTypeConfiguration<Distributor>
{
    public void Configure(EntityTypeBuilder<Distributor> builder)
    {
        builder.ToTable("Distributors");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Phone).HasMaxLength(20);
        builder.Property(d => d.VehicleType).IsRequired().HasMaxLength(30);
        builder.Property(d => d.VehiclePlate).HasMaxLength(20);
        builder.Property(d => d.Notes).HasMaxLength(300);

        builder.HasMany(d => d.Orders)
            .WithOne(o => o.Distributor)
            .HasForeignKey(o => o.DistributorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
