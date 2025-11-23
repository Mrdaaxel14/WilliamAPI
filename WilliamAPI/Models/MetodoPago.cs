using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class MetodoPago
    {
        public int IdMetodoPago { get; set; }
        public string Metodo { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Pedido> Pedidos { get; set; } = new HashSet<Pedido>();
    }
}