using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class RutaConfiguration : IEntityTypeConfiguration<Ruta>
{
    public void Configure(EntityTypeBuilder<Ruta> builder)
    {
        builder.ToTable("Rutas", "acopio");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Codigo).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Status).IsRequired().HasMaxLength(30);
        builder.Property(r => r.Observaciones).HasMaxLength(500);
        builder.Property(r => r.TotalLitrosChofer).HasColumnType("decimal(10,2)");
        builder.Property(r => r.TotalLitrosPlanta).HasColumnType("decimal(10,2)");
        builder.Property(r => r.DiferenciaTotal).HasColumnType("decimal(10,2)");

        builder.HasOne(r => r.Camion)
            .WithMany(c => c.Rutas)
            .HasForeignKey(r => r.CamionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Conductor)
            .WithMany(c => c.Rutas)
            .HasForeignKey(r => r.ConductorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Asistente)
            .WithMany()
            .HasForeignKey(r => r.AsistenteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Recogidas)
            .WithOne(re => re.Ruta)
            .HasForeignKey(re => re.RutaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Fecha);
        builder.HasIndex(r => new { r.CamionId, r.Fecha });
        builder.HasIndex(r => new { r.ConductorId, r.Fecha });
    }
}
