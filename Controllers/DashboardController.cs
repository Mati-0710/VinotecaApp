using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;

namespace VinotecaApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly VinotecaContext _context;

        public DashboardController(VinotecaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Now;

            var ventasDelMes = await _context.Ventas
                .Where(v => v.Fecha.Month == hoy.Month && v.Fecha.Year == hoy.Year)
                .ToListAsync();

            var ventasDelDia = ventasDelMes
                .Where(v => v.Fecha.Date == hoy.Date)
                .ToList();

            var clientes = await _context.Clientes
                .Include(c => c.Movimientos)
                .ToListAsync();

            var clientesConDeuda = clientes
                .Select(c => new ClienteSaldoViewModel
                {
                    NombreCompleto = $"{c.Apellido}, {c.Nombre}",
                    Saldo = c.Movimientos
                        .Sum(m => m.Tipo == "Debito" ? m.Monto : -m.Monto)
                })
                .Where(c => c.Saldo > 0)
                .OrderByDescending(c => c.Saldo)
                .ToList();

            var productosStockBajo = await _context.Productos
                .Where(p => p.Stock <= 5)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalVentasDia = ventasDelDia.Sum(v => v.Total),
                CantidadVentasDia = ventasDelDia.Count,
                TotalVentasMes = ventasDelMes.Sum(v => v.Total),
                CantidadVentasMes = ventasDelMes.Count,
                ClientesConDeuda = clientesConDeuda,
                ProductosStockBajo = productosStockBajo
            };

            return View(model);
        }
    }
}