using WilliamAPI.Models;

namespace WilliamAPI.Models
{
    public class CarritoDetalle
    {
        public int IdCarritoDetalle { get; set; }
        public int IdCarrito { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }

        public virtual Carrito? Carrito { get; set; }
        public virtual Producto? Producto { get; set; }
    }
}
