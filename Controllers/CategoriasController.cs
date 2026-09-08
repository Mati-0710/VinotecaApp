using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;

namespace VinotecaApp.Controllers
{
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
            // Pasamos las categorías padre disponibles para elegir si es una subcategoría
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
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}