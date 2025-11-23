using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class DireccionUsuario
    {
        public int IdDireccion { get; set; }
        public int IdUsuario { get; set; }
        public string? Provincia { get; set; }
        public string? Ciudad { get; set; }
        public string? Calle { get; set; }
        public string? Numero { get; set; }
        public string? CodigoPostal { get; set; }

        [JsonIgnore]
        public Usuario? Usuario { get; set; }
    }
}