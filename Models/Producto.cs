using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VinotecaApp.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }



        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        public int? CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int? BodegaId { get; set; }
        public Bodega? Bodega { get; set; }

        [Range(1900, 2100, ErrorMessage = "Ingresá un año válido")]
        public int? Cosecha { get; set; }

        [Required]
        [Range(1, 1000000, ErrorMessage = "El precio debe ser mayor a 0")]
        public int Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        public string? CategoriaWeb { get; set; }
        [Range(1, 1000000, ErrorMessage = "El precio de oferta debe ser mayor a 0")]
        public int? PrecioOferta { get; set; }

        public ICollection<Coleccion> Colecciones { get; set; } = new List<Coleccion>();

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();

        public bool Activo { get; set; } = true;
    }
}