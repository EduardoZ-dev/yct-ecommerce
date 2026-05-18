using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class GranjeroConfiguration : IEntityTypeConfiguration<Granjero>
{
    public void Configure(EntityTypeBuilder<Granjero> builder)
    {
        builder.ToTable("Granjeros", "acopio");
        builder.HasKey(g => g.Id);
        builder.HasIndex(g => g.Numero).IsUnique();
        builder.Property(g => g.NombreCompleto).IsRequired().HasMaxLength(150);
        builder.Property(g => g.Cedula).HasMaxLength(20);
        builder.Property(g => g.Telefono).HasMaxLength(20);
        builder.Property(g => g.Finca).HasMaxLength(150);
        builder.Property(g => g.Vereda).HasMaxLength(100);
        builder.Property(g => g.Municipio).HasMaxLength(100);
        builder.Property(g => g.PrecioLitro).HasColumnType("decimal(10,2)");
        builder.Property(g => g.PromedioDiario).HasColumnType("decimal(10,2)");
        builder.Property(g => g.Notas).HasMaxLength(500);

        builder.HasIndex(g => g.Cedula).IsUnique().HasFilter("[Cedula] IS NOT NULL");
    }
}
