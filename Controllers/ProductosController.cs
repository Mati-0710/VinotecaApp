using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace VinotecaApp.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        private readonly VinotecaContext _context;

        public ProductosController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Productos
        [HttpGet]
        public async Task<IActionResult> Index(string buscar, int? categoriaId, int? bodegaId)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .Where(p => p.Activo)
                .AsQueryable();

            // 1. Filtro por texto: busca en nombre, bodega y categoría
            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var texto = buscar.Trim();
                query = query.Where(p =>
                    p.Nombre.Contains(texto) ||
                    (p.Bodega != null && p.Bodega.Nombre.Contains(texto)) ||
                    (p.Categoria != null && p.Categoria.Nombre.Contains(texto)));
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

            // Orden alfabético por nombre (lo más práctico para la consulta diaria)
            var productos = await query
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.Categoria != null ? p.Categoria.Nombre : "")
                .ToListAsync();

            ViewBag.CategoriaSeleccionada = categoriaId;
            ViewBag.BodegaSeleccionada = bodegaId;
            ViewBag.Buscar = buscar;

            ViewBag.Categorias = await _context.Categorias
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Bodegas = await _context.Bodegas
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            return View(productos);
        }

        // GET: Productos/Create
        public async Task<IActionResult> Create()
        {
            await CargarListasParaFormulario();
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            await ValidarProducto(producto);

            if (!ModelState.IsValid)
            {
                await CargarListasParaFormulario();
                return View(producto);
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Incluimos las Colecciones para saber cuáles tiene tildadas actualmente
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Bodega)
                .Include(p => p.Colecciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null) return NotFound();

            await CargarListasParaFormulario();
            await CargarColeccionesParaFormulario();

            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto, int[] coleccionesSeleccionadas)
        {
            if (id != producto.Id) return NotFound();

            var seleccionadas = coleccionesSeleccionadas ?? Array.Empty<int>();

            await ValidarProducto(producto);

            if (!ModelState.IsValid)
            {
                // Si hay error, devolvemos la vista conservando los tildes de colecciones
                producto.Colecciones = await _context.Colecciones
                    .Where(c => seleccionadas.Contains(c.Id))
                    .ToListAsync();

                await CargarListasParaFormulario();
                await CargarColeccionesParaFormulario();
                return View(producto);
            }

            // Buscamos el producto original con sus colecciones para actualizar la relación muchos a muchos
            var productoOriginal = await _context.Productos
                .Include(p => p.Colecciones)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productoOriginal == null) return NotFound();

            // Actualizamos los datos básicos
            productoOriginal.Nombre = producto.Nombre;
            productoOriginal.CategoriaId = producto.CategoriaId;
            productoOriginal.BodegaId = producto.BodegaId;
            productoOriginal.Cosecha = producto.Cosecha;
            productoOriginal.Precio = producto.Precio;
            productoOriginal.PrecioOferta = producto.PrecioOferta;
            productoOriginal.Stock = producto.Stock;
            productoOriginal.CategoriaWeb = producto.CategoriaWeb; // marca qué productos van a la landing

            // Reemplazamos las colecciones por las que tildó
            productoOriginal.Colecciones.Clear();
            if (seleccionadas.Length > 0)
            {
                var nuevasColecciones = await _context.Colecciones
                    .Where(c => seleccionadas.Contains(c.Id))
                    .ToListAsync();

                foreach (var col in nuevasColecciones)
                {
                    productoOriginal.Colecciones.Add(col);
                }
            }

            // productoOriginal ya está siendo rastreado por EF: no hace falta _context.Update
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Producto actualizado correctamente.";
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

            // La vista puede usar esto para avisar que el producto se va a ocultar y no a borrar
            ViewBag.TieneVentas = await _context.DetallesVenta.AnyAsync(d => d.ProductoId == id);

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
                // En vez de _context.Productos.Remove(producto);
                producto.Activo = false;

                await _context.SaveChangesAsync();
                TempData["Exito"] = "Producto dado de baja con éxito.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Validaciones de negocio compartidas por Create y Edit
        private async Task ValidarProducto(Producto producto)
        {
            // Evita duplicados por espacios de más ("El Enemigo " vs "El Enemigo")
            producto.Nombre = producto.Nombre?.Trim() ?? "";

            if (producto.CategoriaId == null) return; // ya lo cubre el [Required]

            // El producto debe ir en una subcategoría (hija), no en un tipo general
            var categoria = await _context.Categorias.FindAsync(producto.CategoriaId);
            if (categoria == null || categoria.CategoriaPadreId == null)
            {
                ModelState.AddModelError("CategoriaId",
                    "Elegí una subcategoría (ej: Malbec), no un tipo general.");
            }

            // No permitir un producto idéntico (mismo nombre, categoría, bodega y cosecha)
            var existe = await _context.Productos.AnyAsync(p =>
                p.Id != producto.Id &&
                p.Nombre == producto.Nombre &&
                p.CategoriaId == producto.CategoriaId &&
                p.BodegaId == producto.BodegaId &&
                p.Cosecha == producto.Cosecha);

            if (existe)
            {
                ModelState.AddModelError("Nombre",
                    "Ya existe un producto igual (mismo nombre, categoría, bodega y cosecha).");
            }
        }

        // Carga Categorías (solo padres) y Bodegas en los formularios
        private async Task CargarListasParaFormulario()
        {
            ViewBag.Categorias = await _context.Categorias
                .Where(c => c.CategoriaPadreId == null)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Bodegas = await _context.Bodegas
                .OrderBy(b => b.Nombre)
                .ToListAsync();
        }

        // Carga todas las colecciones disponibles para los checkboxes del Edit
        private async Task CargarColeccionesParaFormulario()
        {
            ViewBag.TodasLasColecciones = await _context.Colecciones
                .OrderBy(c => c.Orden)
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