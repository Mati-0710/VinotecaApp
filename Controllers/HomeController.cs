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
            // Traemos los productos que tienen stock para mostrar en la web
            var productosWeb = await _context.Productos
                .Include(p => p.Bodega)
                .Include(p => p.Categoria)
                .Where(p => p.Stock > 0) 
                // Si querés usar tu propiedad CategoriaWeb para filtrar qué se muestra y qué no, 
                // podés agregar acá algo como: .Where(p => p.CategoriaWeb == true)
                .OrderBy(p => p.Categoria != null ? p.Categoria.Nombre : "Otras")
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            return View(productosWeb);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}