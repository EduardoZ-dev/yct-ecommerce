using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class TinaPlantaConfiguration : IEntityTypeConfiguration<TinaPlanta>
{
    public void Configure(EntityTypeBuilder<TinaPlanta> builder)
    {
        builder.ToTable("TinaPlanta", "acopio");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Cantidad).HasDefaultValue(0);

        // Fila única inicial (la planta arranca con 0 tinas hasta que se ajuste).
        builder.HasData(new TinaPlanta { Id = 1, Cantidad = 0, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
    }
}
