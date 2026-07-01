using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class RecogidaConfiguration : IEntityTypeConfiguration<Recogida>
{
    public void Configure(EntityTypeBuilder<Recogida> builder)
    {
        builder.ToTable("Recogidas", "acopio");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.SaldoChofer).HasColumnType("decimal(6,2)");
        builder.Property(r => r.LitrosChofer).HasColumnType("decimal(10,2)");
        builder.Property(r => r.LitrosRegaladosChofer).HasColumnType("decimal(8,2)");
        builder.Property(r => r.Observacion).HasMaxLength(500);
        builder.Property(r => r.EstadoVista).HasMaxLength(30);
        builder.Property(r => r.EstadoOlor).HasMaxLength(30);
        builder.Property(r => r.EstadoSabor).HasMaxLength(30);
        builder.Property(r => r.SaldoPlanta).HasColumnType("decimal(6,2)");
        builder.Property(r => r.LitrosPlanta).HasColumnType("decimal(10,2)");
        builder.Property(r => r.DiferenciaLitros).HasColumnType("decimal(10,2)");
        builder.Property(r => r.MotivoDiferencia).HasMaxLength(300);

        builder.HasOne(r => r.Ruta)
            .WithMany(ru => ru.Recogidas)
            .HasForeignKey(r => r.RutaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Granjero)
            .WithMany(g => g.Recogidas)
            .HasForeignKey(r => r.GranjeroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.GranjeroCodigo)
            .WithMany()
            .HasForeignKey(r => r.GranjeroCodigoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.OperarioPlantaUser)
            .WithMany()
            .HasForeignKey(r => r.OperarioPlantaUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => new { r.Fecha, r.GranjeroId });
        builder.HasIndex(r => r.RutaId);
        builder.HasIndex(r => r.ClientUuid).IsUnique().HasFilter("[ClientUuid] IS NOT NULL");
    }
}
