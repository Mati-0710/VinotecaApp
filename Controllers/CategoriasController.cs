using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace VinotecaApp.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly VinotecaContext _context;

        public CategoriasController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .Include(c => c.CategoriaPadre)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            return View(categorias);
        }

        // GET: Categorias/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoriasPadre = await _context.Categorias
                .Where(c => c.CategoriaPadreId == null)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoriasPadre = await _context.Categorias
                    .Where(c => c.CategoriaPadreId == null)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
                return View(categoria);
            }

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            ViewBag.CategoriasPadre = await _context.Categorias
                .Where(c => c.CategoriaPadreId == null && c.Id != id)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.CategoriasPadre = await _context.Categorias
                    .Where(c => c.CategoriaPadreId == null && c.Id != id)
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();
                return View(categoria);
            }

            _context.Update(categoria);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // CANDADO 1: ¿Tiene subcategorías colgando?
            bool tieneSubcategorias = await _context.Categorias.AnyAsync(c => c.CategoriaPadreId == id);
            if (tieneSubcategorias)
            {
                TempData["Error"] = "Operación denegada: Esta categoría tiene subcategorías. Borrá o reasigná las subcategorías primero.";
                return RedirectToAction(nameof(Index));
            }

            // CANDADO 2: ¿Tiene productos asociados?
            bool tieneProductos = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            if (tieneProductos)
            {
                TempData["Error"] = "Operación denegada: Hay productos cargados en esta categoría. Cambialos de categoría antes de eliminarla.";
                return RedirectToAction(nameof(Index));
            }

            var categoria = await _context.Categorias
                .Include(c => c.CategoriaPadre)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Verificación extra en el POST por seguridad
            bool enUso = await _context.Categorias.AnyAsync(c => c.CategoriaPadreId == id) || 
                         await _context.Productos.AnyAsync(p => p.CategoriaId == id);
                         
            if (enUso)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque está en uso.";
                return RedirectToAction(nameof(Index));
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Categoría eliminada correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}