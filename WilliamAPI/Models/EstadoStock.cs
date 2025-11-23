using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class EstadoStock
    {
        public int IdEstadoStock { get; set; }
        public string Estado { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Stock> Stocks { get; set; } = new HashSet<Stock>();
    }
}