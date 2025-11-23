using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class MetodoPagoUsuario
    {
        public int IdMetodoPagoUsuario { get; set; }
        public int IdUsuario { get; set; }
        public string Metodo { get; set; } = null!;
        public string? Titular { get; set; }
        public string? Ultimos4 { get; set; }
        public string? Expiracion { get; set; }

        [JsonIgnore]
        public Usuario? Usuario { get; set; }
    }
}