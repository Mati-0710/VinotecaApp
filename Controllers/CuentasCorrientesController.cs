using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace VinotecaApp.Controllers
{
    [Authorize]
    public class CuentasCorrientesController : Controller
    {
        private readonly VinotecaContext _context;

        public CuentasCorrientesController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: CuentasCorrientes (Lista de clientes y sus saldos actuales)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Clientes
                .Include(c => c.MovimientosCuentaCorriente)
                // Ocultamos al Consumidor Final buscando por nombre/apellido
                .Where(c => !c.Apellido.Contains("Consumidor") && !c.Nombre.Contains("Consumidor"))
                .OrderBy(c => c.Apellido)
                .ToListAsync();

            return View(clientes);
        }

        // GET: CuentasCorrientes/Detalle/5 (Historial de movimientos de un cliente)
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.MovimientosCuentaCorriente)
                    .ThenInclude(m => m.Venta)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: CuentasCorrientes/RegistrarPago (Registra una entrega de dinero para bajar la deuda)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(int clienteId, decimal monto)
        {
            if (monto <= 0)
            {
                TempData["Error"] = "El monto del pago debe ser mayor a cero.";
                return RedirectToAction(nameof(Detalle), new { id = clienteId });
            }

            var movimiento = new MovimientoCuentaCorriente
            {
                ClienteId = clienteId,
                Fecha = DateTime.Now,
                Tipo = "Credito", // "Credito" resta a la deuda total
                Monto = monto
            };

            _context.MovimientosCuentaCorriente.Add(movimiento);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pago registrado con éxito.";
            return RedirectToAction(nameof(Detalle), new { id = clienteId });
        }
    }
}