using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }
        public int? IdMetodoPago { get; set; }
        public int? IdEstadoPedido { get; set; }
        public int? IdEstadoPago { get; set; }
        public int? IdDireccion { get; set; }  // ← NUEVO

        [JsonIgnore]
        public Usuario? Usuario { get; set; }

        public MetodoPago? MetodoPago { get; set; }
        public EstadoPedido? EstadoPedido { get; set; }
        public EstadoPago? EstadoPago { get; set; }
        public DireccionUsuario? Direccion { get; set; }  // ← NUEVO

        public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
    }
}