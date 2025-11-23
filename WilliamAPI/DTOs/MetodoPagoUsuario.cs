namespace WilliamAPI.DTOs
{
    public class MetodoPagoUsuarioDto
    {
        public string Metodo { get; set; } = null!;
        public string? Titular { get; set; }
        public string? Ultimos4 { get; set; }
        public string? Expiracion { get; set; }
    }
}