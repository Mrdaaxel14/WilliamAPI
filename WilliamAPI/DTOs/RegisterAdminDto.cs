namespace WilliamAPI.DTOs
{
    public class RegisterAdminDto
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Telefono { get; set; }
        public string CodigoSecreto { get; set; } = null!;
    }
}
