namespace WilliamAPI.DTOs
{
    public class ChangePasswordDto
    {
        public string PasswordActual { get; set; } = null!;
        public string NuevoPassword { get; set; } = null!;
    }
}