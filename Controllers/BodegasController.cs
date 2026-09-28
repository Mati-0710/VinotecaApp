using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace VinotecaApp.Controllers
{
    [Authorize]
    public class BodegasController : Controller
    {
        private readonly VinotecaContext _context;

        public BodegasController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Bodegas
        public async Task<IActionResult> Index()
        {
            var bodegas = await _context.Bodegas.OrderBy(b => b.Nombre).ToListAsync();
            return View(bodegas);
        }

        // GET: Bodegas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bodegas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bodega bodega)
        {
            if (ModelState.IsValid)
            {
                _context.Bodegas.Add(bodega);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bodega);
        }

        // GET: Bodegas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bodega = await _context.Bodegas.FindAsync(id);
            if (bodega == null) return NotFound();

            return View(bodega);
        }

        // POST: Bodegas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bodega bodega)
        {
            if (id != bodega.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bodega);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BodegaExists(bodega.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bodega);
        }

        // GET: Bodegas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // 1. CANDADO PRINCIPAL: Revisamos si hay productos antes de mostrar la pantalla
            bool tieneProductos = await _context.Productos.AnyAsync(p => p.BodegaId == id);
            if (tieneProductos)
            {
                TempData["Error"] = "Operación denegada: La bodega tiene vinos asociados. Si la borrás, se rompe el historial de ventas. Cambiá los vinos de bodega a eliminar.";
                return RedirectToAction(nameof(Index));
            }

            var bodega = await _context.Bodegas.FirstOrDefaultAsync(m => m.Id == id);
            if (bodega == null) return NotFound();

            return View(bodega);
        }

        // POST: Bodegas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 2. Candado secundario por si alguien fuerza el POST
            bool tieneProductos = await _context.Productos.AnyAsync(p => p.BodegaId == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar la bodega porque tiene vinos asociados.";
                return RedirectToAction(nameof(Index));
            }

            var bodega = await _context.Bodegas.FindAsync(id);
            if (bodega != null)
            {
                _context.Bodegas.Remove(bodega);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Bodega eliminada correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BodegaExists(int id)
        {
            return _context.Bodegas.Any(e => e.Id == id);
        }
    }
}