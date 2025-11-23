using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        [Required, MaxLength(100)]
        public string Nombre { get; set; } = null!;
        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;
        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = null!;
        [MaxLength(20)]
        public string Rol { get; set; } = "Cliente";
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public int? IdRol { get; set; }

        [JsonIgnore]
        public Rol? RolNavigation { get; set; }

        [JsonIgnore]
        public ICollection<Carrito> Carritos { get; set; } = new HashSet<Carrito>();
        [JsonIgnore]
        public ICollection<Pedido> Pedidos { get; set; } = new HashSet<Pedido>();
        [JsonIgnore]
        public ICollection<DireccionUsuario> Direcciones { get; set; } = new HashSet<DireccionUsuario>();
        [JsonIgnore]
        public ICollection<MetodoPagoUsuario> MetodosPago { get; set; } = new HashSet<MetodoPagoUsuario>();
        [JsonIgnore]
        public ICollection<Compra> Compras { get; set; } = new HashSet<Compra>();
        [JsonIgnore]
        public ICollection<Auditoria> Auditorias { get; set; } = new HashSet<Auditoria>();
    }
}