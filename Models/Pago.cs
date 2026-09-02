using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinotecaApp.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [Range(0.01, 1000000, ErrorMessage = "El monto debe ser mayor a 0")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(30)]
        public string MedioPago { get; set; } = "Efectivo";
    }
}