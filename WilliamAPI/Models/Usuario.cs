using System.ComponentModel.DataAnnotations;

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
    }
}
