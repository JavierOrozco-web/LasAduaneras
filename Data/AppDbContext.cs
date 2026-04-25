using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> Productos { get; set; }
    public DbSet<Categorias> Categorias { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Empleados> Empleados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var properties = entity.GetProperties()
                .Where(p => p.ClrType == typeof(decimal));

            foreach (var property in properties)
            {
                property.SetPrecision(10);
                property.SetScale(2);
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}