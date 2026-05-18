using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class ConductorConfiguration : IEntityTypeConfiguration<Conductor>
{
    public void Configure(EntityTypeBuilder<Conductor> builder)
    {
        builder.ToTable("Conductores", "acopio");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.NombreCompleto).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Cedula).HasMaxLength(20);
        builder.Property(c => c.Telefono).HasMaxLength(20);

        builder.HasOne(c => c.CamionPreferido)
            .WithMany()
            .HasForeignKey(c => c.CamionPreferidoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.Cedula).IsUnique().HasFilter("[Cedula] IS NOT NULL");
    }
}
