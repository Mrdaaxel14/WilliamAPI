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

        public virtual Categoria? oCategoria { get; set; }
    }
}

