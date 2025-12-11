namespace WilliamAPI.DTOs
{
    public class CrearPedidoDto
    {
        public int IdDireccion { get; set; }
        public int? IdMetodoPagoUsuario { get; set; }  // Método guardado del usuario (opcional)
        public int? IdMetodoPago { get; set; }          // Tipo:  Efectivo=1, Tarjeta=2, MercadoPago=3
    }
}