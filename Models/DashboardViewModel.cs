namespace VinotecaApp.Models
{
    public class DashboardViewModel
    {
        public decimal TotalVentasDia { get; set; }
        public int CantidadVentasDia { get; set; }
        public decimal TotalVentasMes { get; set; }
        public int CantidadVentasMes { get; set; }
        public List<ClienteSaldoViewModel> ClientesConDeuda { get; set; } = new();
        public List<Producto> ProductosStockBajo { get; set; } = new();
    }

    public class ClienteSaldoViewModel
    {
        public string NombreCompleto { get; set; } = "";
        public decimal Saldo { get; set; }
    }
}