using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;

namespace VinotecaApp.Controllers
{
    public class ProductosController : Controller
    {
        private readonly VinotecaContext _context;

        public ProductosController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index(int? categoriaId)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            var productos = await query
                .OrderBy(p => p.Categoria!.Nombre)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            // Para el filtro de arriba de la tabla mostramos TODAS las categorías,
            // incluyendo las agrupadoras (ej: "Vinos"), porque filtrar por "Vinos"
            // tiene que mostrar todos los vinos sin importar la subcategoría.
            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.CategoriaSeleccionada = categoriaId;

            return View(productos);
        }

        // GET: Productos/Create
        public async Task<IActionResult> Create()
        {
            await CargarCategoriasSeleccionables();
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                await CargarCategoriasSeleccionables();
                return View(producto);
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            await CargarCategoriasSeleccionables();
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await CargarCategoriasSeleccionables();
                return View(producto);
            }

            _context.Update(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null) return NotFound();

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Para los formularios de Crear/Editar: solo categorías "hoja" (sin subcategorías propias).
        // "Vinos" queda afuera (es agrupadora); "Malbec", "Cervezas", "Cristalería", etc. quedan adentro.
       private async Task CargarCategoriasSeleccionables()
       {
            ViewBag.Categorias = await _context.Categorias
            .OrderBy(c => c.Nombre)
            .ToListAsync();
        }
    }
}