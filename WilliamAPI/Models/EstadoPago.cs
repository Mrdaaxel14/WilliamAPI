using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class EstadoPago
    {
        public int IdEstadoPago { get; set; }
        public string Estado { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Pedido> Pedidos { get; set; } = new HashSet<Pedido>();
    }
}