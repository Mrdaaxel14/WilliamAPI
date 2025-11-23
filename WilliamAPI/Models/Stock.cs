using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Stock
    {
        public int IdStock { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public int? IdEstadoStock { get; set; }

        public Producto Producto { get; set; } = null!;
        public EstadoStock? EstadoStock { get; set; }
    }
}