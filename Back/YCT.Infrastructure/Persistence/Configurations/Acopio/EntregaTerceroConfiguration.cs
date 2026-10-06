using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class EntregaTerceroConfiguration : IEntityTypeConfiguration<EntregaTercero>
{
    public void Configure(EntityTypeBuilder<EntregaTercero> builder)
    {
        builder.ToTable("EntregasTerceros", "acopio");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Fecha).HasColumnType("date");
        builder.Property(e => e.SaldoLitros).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Litros).HasColumnType("decimal(10,2)");
        builder.Property(e => e.PrecioLitro).HasColumnType("decimal(10,2)");
        builder.Property(e => e.Observacion).HasMaxLength(500);
        builder.Property(e => e.Origen).IsRequired().HasMaxLength(20);
        builder.Property(e => e.RegistradoPorNombre).HasMaxLength(150);
        builder.Property(e => e.ConfirmadaPorNombre).HasMaxLength(150);

        builder.HasOne(e => e.ClienteTercero)
            .WithMany(c => c.Entregas)
            .HasForeignKey(e => e.ClienteTerceroId)
            .OnDelete(DeleteBehavior.Restrict);

        // La ruta es solo el vínculo con el descargue en que llegó la leche: si esa planilla
        // se borra, la entrega del tercero sigue existiendo (es leche que sí entró a planta).
        builder.HasOne<Ruta>()
            .WithMany()
            .HasForeignKey(e => e.RutaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.ClienteTerceroId);
        builder.HasIndex(e => e.Fecha);
        builder.HasIndex(e => e.RutaId);
        builder.HasIndex(e => e.ClientUuid).IsUnique().HasFilter("[ClientUuid] IS NOT NULL");
        // La tablet de planta filtra por "sin confirmar" todos los días: conviene el índice.
        builder.HasIndex(e => e.ConfirmadaEnPlantaAt);
    }
}
