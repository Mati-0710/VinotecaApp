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
        public async Task<IActionResult> Index(int? categoriaId, int? bodegaId, string buscar)
        {
            var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Bodega)
            .AsQueryable();

            // 1. Filtro por texto
            if (!string.IsNullOrEmpty(buscar))
            {
                query = query.Where(p => p.Nombre.Contains(buscar));
            }

            // 2. Filtro por categoría o subcategoría
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value || 
                                       p.Categoria!.CategoriaPadreId == categoriaId.Value);
            }

            // 3. Filtro por Bodega
            if (bodegaId.HasValue)
            {
                query = query.Where(p => p.BodegaId == bodegaId.Value);
            }

            var productos = await query
            .OrderBy(p => p.Categoria!.Nombre)
            .ThenBy(p => p.Nombre)
            .ToListAsync();

            ViewBag.Categorias = await _context.Categorias
            .OrderBy(c => c.Nombre)
            .ToListAsync();

            ViewBag.Bodegas = await _context.Bodegas
            .OrderBy(b => b.Nombre)
            .ToListAsync();

            ViewBag.CategoriaSeleccionada = categoriaId;
            ViewBag.BodegaSeleccionada = bodegaId;
            ViewBag.Buscar = buscar;

            return View(productos);
        }

        // GET: Productos/Create
        public async Task<IActionResult> Create()
        {
            await CargarListasParaFormulario();
            return View();
        }

        // POST: Productos/Create
        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        Console.WriteLine($"-> Key: {modelStateKey} | Error: {error.ErrorMessage}");
                    }
                }

                await CargarListasParaFormulario();
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

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .FirstOrDefaultAsync(p => p.Id == id);
        
            if (producto == null) return NotFound();

            await CargarListasParaFormulario();
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
                await CargarListasParaFormulario();
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
                .Include(p => p.Bodega)
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

        // Método auxiliar unificado para cargar Categorías y Bodegas en los formularios
        private async Task CargarListasParaFormulario()
        {
            // Solo traemos las categorías principales (Padres)
            ViewBag.Categorias = await _context.Categorias
                .Where(c => c.CategoriaPadreId == null)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Bodegas = await _context.Bodegas
                .OrderBy(b => b.Nombre)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<JsonResult> GetSubcategorias(int categoriaPadreId)
        {
             var subcategorias = await _context.Categorias
             .Where(c => c.CategoriaPadreId == categoriaPadreId)
             .Select(c => new { value = c.Id, text = c.Nombre })
             .OrderBy(c => c.text)
             .ToListAsync();

             return Json(subcategorias);
        }
    }
}