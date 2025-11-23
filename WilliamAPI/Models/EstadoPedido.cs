using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class EstadoPedido
    {
        public int IdEstadoPedido { get; set; }
        public string Estado { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Pedido> Pedidos { get; set; } = new HashSet<Pedido>();
    }
}