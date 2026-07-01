using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence.Configurations.Acopio;

public class GranjeroCodigoConfiguration : IEntityTypeConfiguration<GranjeroCodigo>
{
    public void Configure(EntityTypeBuilder<GranjeroCodigo> builder)
    {
        builder.ToTable("GranjeroCodigos", "acopio");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Codigo).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Finca).HasMaxLength(150);
        builder.Property(c => c.TinasYct).HasDefaultValue(0);

        builder.HasOne(c => c.Granjero)
            .WithMany(g => g.Codigos)
            .HasForeignKey(c => c.GranjeroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.GranjeroId);
        builder.HasIndex(c => c.Codigo);
    }
}
