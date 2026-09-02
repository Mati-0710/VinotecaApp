using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinotecaApp.Models
{
    public class MovimientoCuentaCorriente
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public string Tipo { get; set; } = "Debito"; // "Debito" (venta) o "Credito" (pago)

        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        // Nullable: solo se completa si el movimiento vino de una venta
        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }
    }
}