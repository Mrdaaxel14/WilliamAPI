using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Carrito
    {
        public int IdCarrito { get; set; }
        public int IdUsuario { get; set; }

        [JsonIgnore]
        public Usuario? Usuario { get; set; }

        [JsonIgnore]
        public ICollection<CarritoDetalle> Detalles { get; set; } = new List<CarritoDetalle>();
    }
}