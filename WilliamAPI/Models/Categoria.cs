using System.Text.Json.Serialization;
using WilliamAPI.Models;

namespace WilliamAPI.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string? Descripcion { get; set; }

        [JsonIgnore]
        public virtual ICollection<Producto> Productos { get; set; } = new HashSet<Producto>();
    }
}
