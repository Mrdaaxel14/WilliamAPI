using WilliamAPI.Models;

namespace WilliamAPI.Models
{
    public class Carrito
    {
        public int IdCarrito { get; set; }
        public int IdUsuario { get; set; }

        public virtual Usuario? Usuario { get; set; }
        public virtual ICollection<CarritoDetalle> Detalles { get; set; } = new List<CarritoDetalle>();
    }
}
