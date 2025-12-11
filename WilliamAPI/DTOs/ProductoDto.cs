namespace WilliamAPI.DTOs
{
    // DTO para lista de productos (vista resumida)
    public class ProductoListaDto
    {
        public int IdProducto { get; set; }
        public string? CodigoBarra { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Marca { get; set; }
        public int? IdCategoria { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool EnStock { get; set; }
        public string? ImagenPrincipal { get; set; }
    }

    // DTO para detalle de producto (con galería completa)
    public class ProductoDetalleDto
    {
        public int IdProducto { get; set; }
        public string? CodigoBarra { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Marca { get; set; }
        public int? IdCategoria { get; set; }
        public string? CategoriaNombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool EnStock { get; set; }
        public string? ImagenPrincipal { get; set; }
        public List<string> Galeria { get; set; } = new List<string>();
    }

    // DTO para crear/actualizar producto
    public class ProductoCreateUpdateDto
    {
        public string? CodigoBarra { get; set; }
        public string Descripcion { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string? Marca { get; set; }
        public int? IdCategoria { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; } = 0;
    }

    // DTO para agregar/actualizar imágenes
    public class ImagenProductoDto
    {
        public string UrlImagen { get; set; } = null!;
        public bool EsPrincipal { get; set; } = false;
        public int Orden { get; set; } = 0;
    }

    // DTO para actualizar stock manualmente (Admin)
    public class ActualizarStockDto
    {
        public int? CantidadAbsoluta { get; set; }  // Establecer stock directamente
        public int? CantidadAjuste { get; set; }     // Agregar (+) o restar (-) cantidad
    }
}