using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class TinaMovimientoConfiguration : IEntityTypeConfiguration<TinaMovimiento>
{
    public void Configure(EntityTypeBuilder<TinaMovimiento> builder)
    {
        builder.ToTable("TinaMovimientos", "acopio");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Observacion).HasMaxLength(400);
        builder.Property(m => m.UsuarioNombre).HasMaxLength(150);

        builder.HasOne(m => m.GranjeroCodigo)
            .WithMany()
            .HasForeignKey(m => m.GranjeroCodigoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.GranjeroCodigoId);
        builder.HasIndex(m => m.CreatedAt);
    }
}
