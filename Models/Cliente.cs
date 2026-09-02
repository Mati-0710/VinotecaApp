using System.ComponentModel.DataAnnotations;

namespace VinotecaApp.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50)]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El DNI/CUIT es obligatorio")]
        [StringLength(15)]
        public string DniCuit { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? Direccion { get; set; }

        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();

        public ICollection<MovimientoCuentaCorriente> Movimientos { get; set; } = new List<MovimientoCuentaCorriente>();

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}