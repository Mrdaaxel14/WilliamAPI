using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Auditoria
    {
        public int IdAuditoria { get; set; }
        public int? IdUsuario { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string? Accion { get; set; }
        public string? TablaAfectada { get; set; }
        public string? ValorAnterior { get; set; }
        public string? ValorNuevo { get; set; }

        [JsonIgnore]
        public Usuario? Usuario { get; set; }
    }
}