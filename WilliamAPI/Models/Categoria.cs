using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }
        public string Descripcion { get; set; } = null!;

        [JsonIgnore]
        public virtual ICollection<Producto> Productos { get; set; } = new HashSet<Producto>();
    }
}