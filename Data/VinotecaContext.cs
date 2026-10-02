using Microsoft.EntityFrameworkCore;
using VinotecaApp.Models;

namespace VinotecaApp.Data
{
    public class VinotecaContext : DbContext
    {
        public VinotecaContext(DbContextOptions<VinotecaContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        public DbSet<MovimientoCuentaCorriente> MovimientosCuentaCorriente { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<Bodega> Bodegas { get; set; }

        public DbSet<Coleccion> Colecciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Un mismo producto no puede repetirse
            modelBuilder.Entity<Producto>()
                .HasIndex(p => new { p.Nombre, p.CategoriaId, p.BodegaId, p.Cosecha })
                .IsUnique()
                .HasFilter(null);

            // Una subcategoría no puede repetirse dentro del mismo padre
            modelBuilder.Entity<Categoria>()
                .HasIndex(c => new { c.Nombre, c.CategoriaPadreId })
                .IsUnique()
                .HasFilter(null);

            // --- REGLAS DE RESTRINGIDO DE BORRADO (RESTRICT) ---

            // 1. Autorrelación de Categorías (Padre -> Hijas)
            modelBuilder.Entity<Categoria>()
                .HasOne(c => c.CategoriaPadre)
                .WithMany(c => c.Subcategorias)
                .HasForeignKey(c => c.CategoriaPadreId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Categoría -> Productos
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Bodega -> Productos
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Bodega)
                .WithMany(b => b.Productos)
                .HasForeignKey(p => p.BodegaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Cliente -> Ventas
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Cliente -> Movimientos de Cuenta Corriente
            modelBuilder.Entity<MovimientoCuentaCorriente>()
                .HasOne(m => m.Cliente)
                .WithMany(c => c.MovimientosCuentaCorriente)
                .HasForeignKey(m => m.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // 6. Cliente -> Pagos
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}