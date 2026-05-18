using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class AsistenteConfiguration : IEntityTypeConfiguration<Asistente>
{
    public void Configure(EntityTypeBuilder<Asistente> builder)
    {
        builder.ToTable("Asistentes", "acopio");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.NombreCompleto).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Cedula).HasMaxLength(20);
        builder.Property(a => a.Telefono).HasMaxLength(20);

        builder.HasOne(a => a.CamionPreferido)
            .WithMany()
            .HasForeignKey(a => a.CamionPreferidoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.Cedula).IsUnique().HasFilter("[Cedula] IS NOT NULL");
    }
}
