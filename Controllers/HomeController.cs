using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;

namespace VinotecaApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly VinotecaContext _context;

        public HomeController(VinotecaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Colecciones activas, en el orden definido, cada una con sus productos que tienen stock
            var colecciones = await _context.Colecciones
                .AsNoTracking()
                .Where(c => c.Activa)
                .OrderBy(c => c.Orden)
                .Include(c => c.Productos.Where(p => p.Stock > 0).OrderBy(p => p.Nombre))
                    .ThenInclude(p => p.Bodega)
                .ToListAsync();

            // No mostramos colecciones que quedaron sin productos disponibles
            colecciones = colecciones.Where(c => c.Productos.Count > 0).ToList();

            return View(colecciones);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
