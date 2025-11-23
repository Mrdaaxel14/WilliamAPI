using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Compra
    {
        public int IdCompra { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public decimal Total { get; set; }
        public int? IdUsuario { get; set; }

        [JsonIgnore]
        public Usuario? Usuario { get; set; }

        public ICollection<CompraDetalle> Detalles { get; set; } = new HashSet<CompraDetalle>();
    }
}