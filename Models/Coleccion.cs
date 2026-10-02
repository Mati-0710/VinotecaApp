using System.ComponentModel.DataAnnotations;

namespace VinotecaApp.Models
{
    // Agrupación comercial de la landing (ej: "Alta Gama", "Vinos en oferta").
    // Es independiente de Categoria, que es la clasificación interna del sistema.
    public class Coleccion
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        [StringLength(60)]
        public string Titulo { get; set; } = "";

        [StringLength(150)]
        public string? Texto { get; set; }

        [StringLength(300)]
        public string? ImagenUrl { get; set; }

        // Define el orden en que aparecen las cards en la web (menor = primero)
        public int Orden { get; set; }

        // Permite ocultar una colección sin borrarla
        public bool Activa { get; set; } = true;

        // Muchos a muchos: un producto puede estar en varias colecciones.
        // EF Core crea la tabla intermedia (ColeccionProducto) solo, por convención.
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
