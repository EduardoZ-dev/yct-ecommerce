using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class CamionConfiguration : IEntityTypeConfiguration<Camion>
{
    public void Configure(EntityTypeBuilder<Camion> builder)
    {
        builder.ToTable("Camiones", "acopio");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nombre).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Placa).HasMaxLength(20);
        builder.Property(c => c.Estado).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Notas).HasMaxLength(500);

        builder.HasIndex(c => c.Nombre).IsUnique();
    }
}
