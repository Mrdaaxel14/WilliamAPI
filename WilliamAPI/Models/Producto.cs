using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public string? CodigoBarra { get; set; }
        public string Descripcion { get; set; } = null!;
        public string? Marca { get; set; }
        public int? IdCategoria { get; set; }
        public decimal Precio { get; set; }

        [JsonIgnore]
        public Categoria? Categoria { get; set; }
        [JsonIgnore]
        public ICollection<CarritoDetalle> CarritoDetalles { get; set; } = new HashSet<CarritoDetalle>();
        [JsonIgnore]
        public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new HashSet<PedidoDetalle>();
        [JsonIgnore]
        public ICollection<Stock> Stocks { get; set; } = new HashSet<Stock>();
        [JsonIgnore]
        public ICollection<ImagenProducto> Imagenes { get; set; } = new HashSet<ImagenProducto>();
        [JsonIgnore]
        public ICollection<CompraDetalle> CompraDetalles { get; set; } = new HashSet<CompraDetalle>();
    }
}