using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WilliamAPI.Data;
using WilliamAPI.Models;

namespace WilliamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public PedidoController(DBAPIContext db) => _db = db;

        private int GetUserId()
        {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        // POST api/pedido/crear  (Cliente)
        [Authorize(Roles = "Cliente")]
        [HttpPost("crear")]
        public async Task<IActionResult> Crear()
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var carrito = await _db.Carritos.Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null || !carrito.Detalles.Any())
                return BadRequest(new { mensaje = "Carrito vacío" });

            var estadoPedido = await _db.EstadosPedido.AsNoTracking().FirstOrDefaultAsync(e => e.Estado == "Pendiente");
            var estadoPago = await _db.EstadosPago.AsNoTracking().FirstOrDefaultAsync(e => e.Estado == "Pendiente");

            var pedido = new Pedido
            {
                IdUsuario = idUsuario,
                Fecha = DateTime.UtcNow,
                Total = 0m,
                Detalles = new List<PedidoDetalle>(),
                IdEstadoPedido = estadoPedido?.IdEstadoPedido,
                IdEstadoPago = estadoPago?.IdEstadoPago
            };

            decimal total = 0m;
            foreach (var det in carrito.Detalles)
            {
                if (det.Producto == null) continue;
                var producto = det.Producto;
                var precio = producto.Precio;
                var subtotal = det.Cantidad * precio;
                total += subtotal;

                pedido.Detalles.Add(new PedidoDetalle
                {
                    IdProducto = producto.IdProducto,
                    Cantidad = det.Cantidad,
                    PrecioUnitario = precio
                });
            }

            pedido.Total = total;
            _db.Pedidos.Add(pedido);

            // Vaciar carrito (eliminar detalles)
            _db.CarritoDetalles.RemoveRange(carrito.Detalles);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "ok", response = new { pedido.IdPedido, pedido.Total } });
        }

        // GET api/pedido/mis-pedidos (Cliente)
        [Authorize(Roles = "Cliente")]
        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> MisPedidos()
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var pedidos = await _db.Pedidos
                .Where(p => p.IdUsuario == idUsuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            var result = pedidos.Select(p => new
            {
                p.IdPedido,
                p.Fecha,
                p.Total,
                Detalles = p.Detalles.Select(d => new
                {
                    d.IdPedidoDetalle,
                    Producto = d.Producto == null ? null : new { d.Producto.IdProducto, d.Producto.Descripcion, d.Producto.Precio, d.Producto.Marca },
                    d.Cantidad,
                    d.PrecioUnitario
                })
            });

            return Ok(new { mensaje = "ok", response = result });
        }

        // GET api/pedido/todos (Admin) - ver todos los pedidos
        [Authorize(Roles = "Admin")]
        [HttpGet("todos")]
        public async Task<IActionResult> Todos()
        {
            var pedidos = await _db.Pedidos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();

            var result = pedidos.Select(p => new
            {
                p.IdPedido,
                p.Fecha,
                p.Total,
                Usuario = new { p.Usuario!.IdUsuario, p.Usuario!.Nombre, p.Usuario!.Email },
                Detalles = p.Detalles.Select(d => new
                {
                    d.IdPedidoDetalle,
                    Producto = d.Producto == null ? null : new { d.Producto.IdProducto, d.Producto.Descripcion, d.Producto.Precio },
                    d.Cantidad,
                    d.PrecioUnitario
                })
            });

            return Ok(new { mensaje = "ok", response = result });
        }
    }
}