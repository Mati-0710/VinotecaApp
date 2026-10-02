using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VinotecaApp.Data;
using VinotecaApp.Models;
using Microsoft.AspNetCore.Authorization;

namespace VinotecaApp.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly VinotecaContext _context;

        public ClientesController(VinotecaContext context)
        {
            _context = context;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            // Le pasamos una alerta a la vista si el cliente fue dado de baja
            if (!cliente.Activo)
            {
                ViewBag.AlertaInactivo = "Este cliente se encuentra dado de baja. Estás viendo su registro histórico.";
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Apellido,DniCuit,Telefono,Email,Direccion")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }

            // CANDADO GET: Evita que alguien entre a la pantalla de edición escribiendo la URL
            if (cliente.EsConsumidorFinal)
            {
                TempData["Error"] = "Seguridad: No se puede editar el registro de sistema 'Consumidor Final'.";
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,DniCuit,Telefono,Email,Direccion")] Cliente clienteFormulario)
        {
            if (id != clienteFormulario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Traemos el cliente real de la base de datos (lo trae con Activo = true)
                    var clienteReal = await _context.Clientes.FindAsync(id);
                    if (clienteReal == null)
                    {
                        return NotFound();
                    }

                    // 2. CANDADO POST: Si es Consumidor Final, lo rebotamos
                    if (clienteReal.EsConsumidorFinal)
                    {
                        TempData["Error"] = "Alerta de Seguridad: Intento de modificación a registro protegido.";
                        return RedirectToAction(nameof(Index));
                    }

                    // 3. Pasamos solo los datos que Leandro editó, sin tocar ni el Activo ni las colecciones
                    clienteReal.Nombre = clienteFormulario.Nombre;
                    clienteReal.Apellido = clienteFormulario.Apellido;
                    clienteReal.DniCuit = clienteFormulario.DniCuit;
                    clienteReal.Telefono = clienteFormulario.Telefono;
                    clienteReal.Email = clienteFormulario.Email;
                    clienteReal.Direccion = clienteFormulario.Direccion;

                    // 4. Guardamos los cambios
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(clienteFormulario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(clienteFormulario);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            // CANDADO GET: Evita que alguien llegue a la pantalla de "Confirmar Eliminación"
            if (cliente.EsConsumidorFinal)
            {
                TempData["Error"] = "Seguridad: No se puede eliminar el registro de sistema 'Consumidor Final'.";
                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Traemos al cliente incluyendo sus movimientos para calcular el saldo
            var cliente = await _context.Clientes
                .Include(c => c.MovimientosCuentaCorriente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente != null)
            {
                // Candado 1: El Consumidor Final no se toca
                if (cliente.EsConsumidorFinal)
                {
                    TempData["Error"] = "Seguridad: No se puede dar de baja al 'Consumidor Final'.";
                    return RedirectToAction(nameof(Index));
                }

                // Candado 2: Cálculo de saldo pendiente usando el campo Tipo de tu modelo
                decimal totalDebitos = cliente.MovimientosCuentaCorriente
                    .Where(m => m.Tipo == "Debito")
                    .Sum(m => m.Monto);

                decimal totalCreditos = cliente.MovimientosCuentaCorriente
                    .Where(m => m.Tipo == "Credito")
                    .Sum(m => m.Monto);

                decimal saldoPendiente = totalDebitos - totalCreditos;

                // Si debe plata (saldo mayor a 0), frenamos la baja
                if (saldoPendiente > 0)
                {
                    TempData["Error"] = $"No se puede dar de baja a {cliente.Nombre} {cliente.Apellido} porque tiene un saldo deudor de ${saldoPendiente:N2}.";
                    return RedirectToAction(nameof(Index));
                }

                // Si pasó las validaciones, lo damos de baja lógicamente
                cliente.Activo = false;
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Cliente dado de baja con éxito.";
            }

            return RedirectToAction(nameof(Index));
        }
        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
