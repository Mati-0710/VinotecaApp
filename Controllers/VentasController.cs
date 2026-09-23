using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace VinotecaApp.Controllers
{
    [Authorize]
    public class VentasController : Controller
    {
        private readonly VinotecaContext _context;

        public VentasController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Ventas
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await CargarListasDesplegablesAsync();

            // Historial de ventas trayendo cliente y los detalles con sus productos
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            ViewBag.Ventas = ventas;

            // Buscamos al Consumidor Final de forma inequívoca por su bandera booleana
            var consumidorFinal = await _context.Clientes
                .FirstOrDefaultAsync(c => c.EsConsumidorFinal);

            var model = new VentaCreateViewModel
            {
                Fecha = DateTime.Now,
                ClienteId = consumidorFinal?.Id ?? 0
            };

            return View(model);
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaCreateViewModel model)
        {
            // 1. Si no se eligió cliente o llegó en 0, asignamos el Consumidor Final por defecto
            if (model.ClienteId == 0)
            {
                var consumidorFinal = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.EsConsumidorFinal);
                    
                if (consumidorFinal != null)
                {
                    model.ClienteId = consumidorFinal.Id;
                }
            }

            // 2. Agrupar líneas para evitar duplicados de stock y filtrar vacíos
            var lineasProcesadas = model.Lineas
                .Where(l => l.ProductoId.HasValue && l.Cantidad.HasValue && l.Cantidad > 0)
                .GroupBy(l => l.ProductoId!.Value)
                .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad!.Value) })
                .ToList();

            if (!lineasProcesadas.Any())
            {
                ModelState.AddModelError("", "Debe cargar al menos un producto en la venta.");
            }
            // Validación extra: El Consumidor Final no puede usar Cuenta Corriente
            var consumidorFinalDb = await _context.Clientes.FirstOrDefaultAsync(c => c.EsConsumidorFinal);
            if (model.ClienteId == consumidorFinalDb?.Id && model.MedioPago == "CuentaCorriente")
            {
                ModelState.AddModelError("", "El Consumidor Final no puede tener Cuenta Corriente. Seleccione Efectivo, Tarjeta o Transferencia.");
            }

            if (!ModelState.IsValid)
            {
                await CargarListasDesplegablesAsync();
                ViewBag.Ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();
                return View("Index", model);
            }

            // 3. Transacción para asegurar atomicidad (Todo o Nada)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = new Venta
                {
                    ClienteId = model.ClienteId,
                    MedioPago = model.MedioPago,
                    Fecha = DateTime.Now,
                    Total = 0 
                };

                decimal calculoTotalSistema = 0;

                foreach (var item in lineasProcesadas)
                {
                    var producto = await _context.Productos.FindAsync(item.ProductoId);

                    if (producto == null)
                    {
                        ModelState.AddModelError("", "Uno de los productos seleccionados no existe.");
                        await transaction.RollbackAsync();
                        await RecargarVistaErrorAsync(model);
                        return View("Index", model);
                    }

                    if (producto.Stock < item.Cantidad)
                    {
                        ModelState.AddModelError("", $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}.");
                        await transaction.RollbackAsync();
                        await RecargarVistaErrorAsync(model);
                        return View("Index", model);
                    }

                    decimal subtotalLinea = producto.Precio * item.Cantidad;
                    calculoTotalSistema += subtotalLinea;

                    var detalle = new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = producto.Precio,
                        Subtotal = subtotalLinea
                    };

                    venta.Detalles.Add(detalle);
                    producto.Stock -= item.Cantidad;
                }

                // Respetamos el total editado por Leandro si aplicó promoción, caso contrario usamos el del sistema
                venta.Total = model.TotalFinal > 0 ? model.TotalFinal : calculoTotalSistema;
                _context.Ventas.Add(venta);

                // Si es cuenta corriente, generamos el movimiento de débito
                if (model.MedioPago == "CuentaCorriente")
                {
                    var movimiento = new MovimientoCuentaCorriente
                    {
                        ClienteId = model.ClienteId,
                        Fecha = DateTime.Now,
                        Tipo = "Debito",
                        Monto = venta.Total,
                        Venta = venta
                    };
                    _context.MovimientosCuentaCorriente.Add(movimiento);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Ocurrió un error inesperado al procesar la venta.");
                await RecargarVistaErrorAsync(model);
                return View("Index", model);
            }
        }

        // Métodos auxiliares privados para mantener limpio el código
        private async Task CargarListasDesplegablesAsync()
        {
            ViewBag.Clientes = await _context.Clientes
                .OrderBy(c => c.Apellido)
                .ToListAsync();

            ViewBag.Productos = await _context.Productos
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        private async Task RecargarVistaErrorAsync(VentaCreateViewModel model)
        {
            await CargarListasDesplegablesAsync();
            ViewBag.Ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductosAjax(string q)
        {
            var query = _context.Productos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(p => p.Nombre.Contains(q));
            }

        // Traemos solo los 20 primeros resultados que tengan stock para que sea rapidísimo
        var productos = await query
        .Where(p => p.Stock > 0)
        .Take(20)
        .Select(p => new {
            id = p.Id,
            text = p.Nombre,
            precio = p.Precio
        })
        .ToListAsync();

        return Json(new { results = productos });
        }
    }
}