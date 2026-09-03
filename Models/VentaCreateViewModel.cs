using System.ComponentModel.DataAnnotations;

namespace VinotecaApp.Models
{
    public class VentaCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente")]
        public int ClienteId { get; set; }

        [Required]
        public string MedioPago { get; set; } = "Efectivo";

        // Agregamos esta propiedad para capturar el total editado en la vista
        public decimal TotalFinal { get; set; }

        public List<LineaVentaViewModel> Lineas { get; set; } = new List<LineaVentaViewModel>
        {
            new LineaVentaViewModel(),
            new LineaVentaViewModel(),
            new LineaVentaViewModel(),
            new LineaVentaViewModel()
        };
    }

    public class LineaVentaViewModel
    {
        public int? ProductoId { get; set; }
        public int? Cantidad { get; set; }
    }
}