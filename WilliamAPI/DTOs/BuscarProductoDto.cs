namespace WilliamAPI.DTOs
{
    public class BuscarProductoResultDto
    {
        public int TotalItems { get; set; }
        public int TotalPaginas { get; set; }
        public int PaginaActual { get; set; }
        public int ItemsPorPagina { get; set; }
        public List<ProductoListaDto> Productos { get; set; } = new();
    }
}