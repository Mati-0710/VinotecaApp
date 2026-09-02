using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;

namespace VinotecaApp.Controllers
{
    public class PagosController : Controller
    {
        private readonly VinotecaContext _context;

        public PagosController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Pagos
        public async Task<IActionResult> Index()
        {
            var pagos = await _context.Pagos
                .Include(p => p.Cliente)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
            return View(pagos);
        }

        // GET: Pagos/Create
        public IActionResult Create()
        {
            CargarClientes();
            return View(new Pago());
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pago pago)
        {
            if (!ModelState.IsValid)
            {
                CargarClientes();
                return View(pago);
            }

            pago.Fecha = DateTime.Now;
            _context.Pagos.Add(pago);

            var movimiento = new MovimientoCuentaCorriente
            {
                ClienteId = pago.ClienteId,
                Fecha = pago.Fecha,
                Tipo = "Credito",
                Monto = pago.Monto,
                VentaId = null
            };
            _context.MovimientosCuentaCorriente.Add(movimiento);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void CargarClientes()
        {
            ViewBag.Clientes = _context.Clientes
                .OrderBy(c => c.Apellido)
                .Select(c => new { c.Id, NombreCompleto = c.Apellido + ", " + c.Nombre })
                .ToList();
        }
    }
}