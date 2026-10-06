using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class ClienteTerceroConfiguration : IEntityTypeConfiguration<ClienteTercero>
{
    public void Configure(EntityTypeBuilder<ClienteTercero> builder)
    {
        builder.ToTable("ClientesTerceros", "acopio");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.NombreCompleto).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Cedula).HasMaxLength(20);
        builder.Property(c => c.Telefono).HasMaxLength(20);
        builder.Property(c => c.Municipio).HasMaxLength(100);
        builder.Property(c => c.PrecioLitro).HasColumnType("decimal(10,2)");
        builder.Property(c => c.Notas).HasMaxLength(500);

        builder.HasIndex(c => c.Cedula).IsUnique().HasFilter("[Cedula] IS NOT NULL");
        builder.HasIndex(c => c.NombreCompleto);
    }
}
