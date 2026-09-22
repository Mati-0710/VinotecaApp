using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinotecaApp.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        [Required]
        public int VentaId { get; set; }
        public Venta? Venta { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }
}