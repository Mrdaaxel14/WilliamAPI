namespace WilliamAPI.DTOs
{
    public class UpdateUsuarioDto
    {
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Rol { get; set; }
        public int? IdRol { get; set; }
    }
}