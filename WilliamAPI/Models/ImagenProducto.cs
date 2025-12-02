using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class ImagenProducto
    {
        public int IdImagen { get; set; }
        public int IdProducto { get; set; }
        public string UrlImagen { get; set; } = null!;
        public int Orden { get; set; } = 0; // 0 = principal, 1+ = galería
        public bool EsPrincipal { get; set; } = false;

        [JsonIgnore]
        public Producto Producto { get; set; } = null!;
    }
}