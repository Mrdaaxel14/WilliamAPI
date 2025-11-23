using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class ImagenProducto
    {
        public int IdImagen { get; set; }
        public int IdProducto { get; set; }
        public string UrlImagen { get; set; } = null!;

        [JsonIgnore]
        public Producto Producto { get; set; } = null!;
    }
}