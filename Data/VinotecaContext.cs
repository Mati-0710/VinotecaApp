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
    }
}