using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class CarritoDetalle
    {
        public int IdCarritoDetalle { get; set; }
        public int IdCarrito { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }

        [JsonIgnore]
        public Carrito? Carrito { get; set; }

        public Producto? Producto { get; set; }
    }
}