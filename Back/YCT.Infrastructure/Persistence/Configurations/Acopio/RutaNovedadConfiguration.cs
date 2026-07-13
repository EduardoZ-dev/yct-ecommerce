using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class RutaNovedadConfiguration : IEntityTypeConfiguration<RutaNovedad>
{
    public void Configure(EntityTypeBuilder<RutaNovedad> builder)
    {
        builder.ToTable("RutaNovedades", "acopio");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Uuid).HasMaxLength(64).IsRequired();
        builder.Property(n => n.PlanillaUuid).HasMaxLength(64).IsRequired();
        builder.Property(n => n.Categoria).HasMaxLength(30).IsRequired();
        builder.Property(n => n.Tipo).HasMaxLength(60).IsRequired();
        builder.Property(n => n.Descripcion).HasMaxLength(500);
        builder.Property(n => n.GpsLat).HasColumnType("decimal(10,6)");
        builder.Property(n => n.GpsLng).HasColumnType("decimal(10,6)");

        builder.HasOne(n => n.Ruta)
            .WithMany()
            .HasForeignKey(n => n.RutaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.Conductor).WithMany().HasForeignKey(n => n.ConductorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(n => n.Camion).WithMany().HasForeignKey(n => n.CamionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(n => n.GranjeroCodigo).WithMany().HasForeignKey(n => n.GranjeroCodigoId).OnDelete(DeleteBehavior.SetNull);

        // Idempotencia: la tablet reenvía hasta confirmar; el mismo UUID no puede duplicarse.
        builder.HasIndex(n => n.Uuid).IsUnique();
        // Para ligar las novedades a la ruta cuando la planilla por fin llega.
        builder.HasIndex(n => n.PlanillaUuid);
        builder.HasIndex(n => n.RutaId);
    }
}
