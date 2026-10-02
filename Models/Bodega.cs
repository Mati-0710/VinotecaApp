namespace VinotecaApp.Models
{
    public class Bodega
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        // Agregamos la colección para evitar FK fantasmas
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}