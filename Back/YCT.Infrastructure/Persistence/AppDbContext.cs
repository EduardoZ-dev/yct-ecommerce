using Microsoft.EntityFrameworkCore;
using YCT.Domain.Entities;
using YCT.Domain.Entities.Acopio;

namespace YCT.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ===== E-commerce =====
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Distributor> Distributors => Set<Distributor>();

    // ===== Acopio lechero =====
    public DbSet<Camion> Camiones => Set<Camion>();
    public DbSet<Conductor> Conductores => Set<Conductor>();
    public DbSet<Asistente> Asistentes => Set<Asistente>();
    public DbSet<Granjero> Granjeros => Set<Granjero>();
    public DbSet<Ruta> Rutas => Set<Ruta>();
    public DbSet<Recogida> Recogidas => Set<Recogida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
