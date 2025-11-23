using System.Text.Json.Serialization;

namespace WilliamAPI.Models
{
    public class CompraDetalle
    {
        public int IdCompraDetalle { get; set; }
        public int IdCompra { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }

        [JsonIgnore]
        public Compra? Compra { get; set; }

        public Producto? Producto { get; set; }
    }
}