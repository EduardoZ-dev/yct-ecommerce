using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YCT.Domain.Entities;

namespace YCT.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Username).IsRequired().HasMaxLength(50);
        builder.Property(a => a.UserRole).IsRequired().HasMaxLength(30);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Summary).IsRequired().HasMaxLength(500);
        builder.Property(a => a.Details).HasColumnType("nvarchar(max)");
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.UserAgent).HasMaxLength(300);

        builder.HasOne(a => a.User)
               .WithMany()
               .HasForeignKey(a => a.UserId)
               .OnDelete(DeleteBehavior.SetNull);

        // Índices para filtros comunes
        builder.HasIndex(a => a.CreatedAt);
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.Action);
    }
}
