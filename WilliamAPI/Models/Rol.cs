using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Rol
    {
        public int IdRol { get; set; }
        public string Nombre { get; set; } = null!;

        [JsonIgnore]
        public ICollection<Usuario> Usuarios { get; set; } = new HashSet<Usuario>();
    }
}
