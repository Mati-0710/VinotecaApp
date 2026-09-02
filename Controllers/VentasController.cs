using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;

namespace VinotecaApp.Controllers
{
    public class VentasController : Controller
    {
        private readonly VinotecaContext _context;

        public VentasController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            CargarListasDesplegables();

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            ViewBag.Ventas = ventas;

            return View(new VentaCreateViewModel());
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaCreateViewModel model)
        {
            var lineasCargadas = model.Lineas
                .Where(l => l.ProductoId.HasValue && l.Cantidad.HasValue && l.Cantidad > 0)
                .ToList();

            if (!lineasCargadas.Any())
            {
                ModelState.AddModelError("", "Debe cargar al menos un producto en la venta.");
            }

            if (!ModelState.IsValid)
            {
                CargarListasDesplegables();
                ViewBag.Ventas = await _context.Ventas
                    .Include(v => v.Cliente)
                    .OrderByDescending(v => v.Fecha)
                    .ToListAsync();
                return View("Index", model);
            }

            var venta = new Venta
            {
                ClienteId = model.ClienteId,
                MedioPago = model.MedioPago,
                Fecha = DateTime.Now
            };

            decimal totalVenta = 0;

            foreach (var linea in lineasCargadas)
            {
                var producto = await _context.Productos.FindAsync(linea.ProductoId);

                if (producto == null)
                {
                    ModelState.AddModelError("", "Producto no encontrado.");
                    CargarListasDesplegables();
                    ViewBag.Ventas = await _context.Ventas.Include(v => v.Cliente).OrderByDescending(v => v.Fecha).ToListAsync();
                    return View("Index", model);
                }

                if (producto.Stock < linea.Cantidad)
                {
                    ModelState.AddModelError("", $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}.");
                    CargarListasDesplegables();
                    ViewBag.Ventas = await _context.Ventas.Include(v => v.Cliente).OrderByDescending(v => v.Fecha).ToListAsync();
                    return View("Index", model);
                }

                var detalle = new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = linea.Cantidad!.Value,
                    PrecioUnitario = producto.Precio,
                    Subtotal = producto.Precio * linea.Cantidad.Value
                };

                venta.Detalles.Add(detalle);
                totalVenta += detalle.Subtotal;
                producto.Stock -= linea.Cantidad.Value;
            }

            venta.Total = totalVenta;
            _context.Ventas.Add(venta);

            if (model.MedioPago == "CuentaCorriente")
            {
                var movimiento = new MovimientoCuentaCorriente
                {
                    ClienteId = model.ClienteId,
                    Fecha = DateTime.Now,
                    Tipo = "Debito",
                    Monto = totalVenta,
                    Venta = venta
                };
                _context.MovimientosCuentaCorriente.Add(movimiento);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void CargarListasDesplegables()
        {
            ViewBag.Clientes = _context.Clientes
                .OrderBy(c => c.Apellido)
                .Select(c => new { c.Id, NombreCompleto = c.Apellido + ", " + c.Nombre })
                .ToList();

            ViewBag.Productos = _context.Productos
                .OrderBy(p => p.Nombre)
                .ToList();
        }
    }
}