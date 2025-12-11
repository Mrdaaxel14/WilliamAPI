using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WilliamAPI.Data;
using WilliamAPI.DTOs;
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
            var idClaim = User.FindFirst("id")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        // POST api/pedido/crear (Cliente)
        [Authorize(Roles = "Cliente")]
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearPedidoDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            // Validar dirección
            var direccion = await _db.DireccionesUsuario
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDireccion == dto.IdDireccion && d.IdUsuario == idUsuario);

            if (direccion == null)
                return BadRequest(new { mensaje = "Dirección no válida o no pertenece al usuario" });

            // Validar método de pago (si se envía IdMetodoPagoUsuario)
            if (dto.IdMetodoPagoUsuario.HasValue)
            {
                var metodoPagoUsuario = await _db.MetodosPagoUsuario
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.IdMetodoPagoUsuario == dto.IdMetodoPagoUsuario && m.IdUsuario == idUsuario);

                if (metodoPagoUsuario == null)
                    return BadRequest(new { mensaje = "Método de pago no válido o no pertenece al usuario" });
            }

            // Validar tipo de método de pago (Efectivo, Tarjeta, etc.)
            if (dto.IdMetodoPago.HasValue)
            {
                var metodoPago = await _db.MetodosPago
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.IdMetodoPago == dto.IdMetodoPago);

                if (metodoPago == null)
                    return BadRequest(new { mensaje = "Tipo de método de pago no válido" });
            }

            // Obtener carrito
            var carrito = await _db.Carritos
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null || !carrito.Detalles.Any())
                return BadRequest(new { mensaje = "Carrito vacío" });

            // Validar stock de todos los productos
            var erroresStock = new List<string>();
            foreach (var det in carrito.Detalles)
            {
                if (det.Producto == null) continue;

                if (det.Producto.Stock < det.Cantidad)
                {
                    erroresStock.Add($"'{det.Producto.Nombre}': solicitado {det.Cantidad}, disponible {det.Producto.Stock}");
                }
            }

            if (erroresStock.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Stock insuficiente para algunos productos",
                    errores = erroresStock
                });
            }

            // Obtener estados por defecto
            var estadoPedido = await _db.EstadosPedido
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Estado == "Pendiente");
            var estadoPago = await _db.EstadosPago
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Estado == "Pendiente");

            // Crear pedido
            var pedido = new Pedido
            {
                IdUsuario = idUsuario,
                Fecha = DateTime.UtcNow,
                Total = 0m,
                Detalles = new List<PedidoDetalle>(),
                IdEstadoPedido = estadoPedido?.IdEstadoPedido,
                IdEstadoPago = estadoPago?.IdEstadoPago,
                IdDireccion = dto.IdDireccion,
                IdMetodoPago = dto.IdMetodoPago
            };

            decimal total = 0m;
            foreach (var det in carrito.Detalles)
            {
                if (det.Producto == null) continue;

                var precio = det.Producto.Precio;
                var subtotal = det.Cantidad * precio;
                total += subtotal;

                pedido.Detalles.Add(new PedidoDetalle
                {
                    IdProducto = det.Producto.IdProducto,
                    Cantidad = det.Cantidad,
                    PrecioUnitario = precio
                });

                // Descontar stock del producto
                var productoParaActualizar = await _db.Productos.FindAsync(det.IdProducto);
                if (productoParaActualizar != null)
                {
                    productoParaActualizar.Stock -= det.Cantidad;
                }
            }

            pedido.Total = total;
            _db.Pedidos.Add(pedido);

            // Vaciar carrito
            _db.CarritoDetalles.RemoveRange(carrito.Detalles);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Pedido creado exitosamente",
                response = new
                {
                    pedido.IdPedido,
                    pedido.Total,
                    pedido.Fecha,
                    Estado = estadoPedido?.Estado ?? "Pendiente"
                }
            });
        }

        // GET api/pedido/{id} (Cliente - ver detalle de un pedido)
        [Authorize(Roles = "Cliente")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPedido(int id)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var pedido = await _db.Pedidos
                .AsNoTracking()
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(pr => pr!.Imagenes)
                .Include(p => p.EstadoPedido)
                .Include(p => p.EstadoPago)
                .Include(p => p.MetodoPago)
                .Include(p => p.Direccion)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuario == idUsuario);

            if (pedido == null)
                return NotFound(new { mensaje = "Pedido no encontrado" });

            var result = new
            {
                pedido.IdPedido,
                pedido.Fecha,
                pedido.Total,
                EstadoPedido = pedido.EstadoPedido?.Estado,
                EstadoPago = pedido.EstadoPago?.Estado,
                MetodoPago = pedido.MetodoPago?.Metodo,
                Direccion = pedido.Direccion == null ? null : new
                {
                    pedido.Direccion.IdDireccion,
                    pedido.Direccion.Provincia,
                    pedido.Direccion.Ciudad,
                    pedido.Direccion.Calle,
                    pedido.Direccion.Numero,
                    pedido.Direccion.CodigoPostal
                },
                Detalles = pedido.Detalles.Select(d => new
                {
                    d.IdPedidoDetalle,
                    d.Cantidad,
                    d.PrecioUnitario,
                    Subtotal = d.Cantidad * d.PrecioUnitario,
                    Producto = d.Producto == null ? null : new
                    {
                        d.Producto.IdProducto,
                        d.Producto.Descripcion,
                        d.Producto.Marca,
                        d.Producto.Precio,
                        ImagenPrincipal = d.Producto.Imagenes
                            .Where(i => i.EsPrincipal)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                            ?? d.Producto.Imagenes
                                .OrderBy(i => i.Orden)
                                .Select(i => i.UrlImagen)
                                .FirstOrDefault()
                    }
                })
            };

            return Ok(new { mensaje = "ok", response = result });
        }

        // GET api/pedido/mis-pedidos (Cliente)
        [Authorize(Roles = "Cliente")]
        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> MisPedidos()
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var pedidos = await _db.Pedidos
                .AsNoTracking()
                .Where(p => p.IdUsuario == idUsuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(pr => pr!.Imagenes)
                .Include(p => p.EstadoPedido)
                .Include(p => p.EstadoPago)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            var result = pedidos.Select(p => new
            {
                p.IdPedido,
                p.Fecha,
                p.Total,
                EstadoPedido = p.EstadoPedido?.Estado,
                EstadoPago = p.EstadoPago?.Estado,
                CantidadItems = p.Detalles.Sum(d => d.Cantidad),
                Detalles = p.Detalles.Select(d => new
                {
                    d.IdPedidoDetalle,
                    Producto = d.Producto == null ? null : new
                    {
                        d.Producto.IdProducto,
                        d.Producto.Descripcion,
                        d.Producto.Precio,
                        d.Producto.Marca,
                        ImagenPrincipal = d.Producto.Imagenes
                            .Where(i => i.EsPrincipal)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                    },
                    d.Cantidad,
                    d.PrecioUnitario
                })
            });

            return Ok(new { mensaje = "ok", response = result });
        }

        // PUT api/pedido/{id}/cancelar (Cliente)
        [Authorize(Roles = "Cliente")]
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> CancelarPedido(int id)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var pedido = await _db.Pedidos
                .Include(p => p.EstadoPedido)
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.IdPedido == id && p.IdUsuario == idUsuario);

            if (pedido == null)
                return NotFound(new { mensaje = "Pedido no encontrado" });

            // Solo se puede cancelar si está en estado "Pendiente" o "Confirmado"
            var estadosPermitidos = new[] { "Pendiente", "Confirmado" };
            if (pedido.EstadoPedido != null && !estadosPermitidos.Contains(pedido.EstadoPedido.Estado))
            {
                return BadRequest(new { mensaje = $"No se puede cancelar un pedido en estado '{pedido.EstadoPedido.Estado}'" });
            }

            // Cambiar estado a Cancelado
            var estadoCancelado = await _db.EstadosPedido.FirstOrDefaultAsync(e => e.Estado == "Cancelado");
            if (estadoCancelado != null)
            {
                pedido.IdEstadoPedido = estadoCancelado.IdEstadoPedido;
            }

            // Restaurar stock
            foreach (var det in pedido.Detalles)
            {
                var producto = await _db.Productos.FindAsync(det.IdProducto);
                if (producto != null)
                {
                    producto.Stock += det.Cantidad;
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Pedido cancelado exitosamente" });
        }

        // GET api/pedido/todos (Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet("todos")]
        public async Task<IActionResult> Todos()
        {
            var pedidos = await _db.Pedidos
                .AsNoTracking()
                .Include(p => p.Usuario)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Include(p => p.EstadoPedido)
                .Include(p => p.EstadoPago)
                .Include(p => p.MetodoPago)
                .Include(p => p.Direccion)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            var result = pedidos.Select(p => new
            {
                p.IdPedido,
                p.Fecha,
                p.Total,
                EstadoPedido = p.EstadoPedido?.Estado,
                EstadoPago = p.EstadoPago?.Estado,
                MetodoPago = p.MetodoPago?.Metodo,
                Usuario = p.Usuario == null ? null : new { p.Usuario.IdUsuario, p.Usuario.Nombre, p.Usuario.Email },
                Direccion = p.Direccion == null ? null : new
                {
                    p.Direccion.Provincia,
                    p.Direccion.Ciudad,
                    p.Direccion.Calle,
                    p.Direccion.Numero
                },
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

        // PUT api/pedido/{id}/estado (Admin - cambiar estado del pedido)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoPedidoDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var pedido = await _db.Pedidos
                    .Include(p => p.Detalles)
                    .Include(p => p.EstadoPedido)
                    .FirstOrDefaultAsync(p => p.IdPedido == id);

                if (pedido == null)
                    return NotFound(new { mensaje = "Pedido no encontrado" });

                // Obtener estados de cancelado y devuelto
                var estadoCancelado = await _db.EstadosPedido.FirstOrDefaultAsync(e => e.Estado == "Cancelado");
                var estadoDevuelto = await _db.EstadosPedido.FirstOrDefaultAsync(e => e.Estado == "Devuelto");
                var estadosCanceladosIds = new List<int>();
                if (estadoCancelado != null) estadosCanceladosIds.Add(estadoCancelado.IdEstadoPedido);
                if (estadoDevuelto != null) estadosCanceladosIds.Add(estadoDevuelto.IdEstadoPedido);

                var estabaAnulado = estadosCanceladosIds.Contains(pedido.IdEstadoPedido ?? 0);

                // Actualizar estado de pedido
                if (dto.IdEstadoPedido.HasValue)
                {
                    var estado = await _db.EstadosPedido.FindAsync(dto.IdEstadoPedido.Value);
                    if (estado == null)
                        return BadRequest(new { mensaje = "Estado de pedido no válido" });

                    var seAnula = estadosCanceladosIds.Contains(dto.IdEstadoPedido.Value);

                    // Lógica de gestión de stock según cambio de estado
                    if (!estabaAnulado && seAnula)
                    {
                        // Devolver stock (el pedido se está cancelando o devolviendo)
                        foreach (var det in pedido.Detalles)
                        {
                            var producto = await _db.Productos.FindAsync(det.IdProducto);
                            if (producto != null)
                            {
                                producto.Stock += det.Cantidad;
                            }
                        }

                        // Registrar en auditoría
                        var idUsuario = User.FindFirst("id")?.Value ?? "0";
                        if (int.TryParse(idUsuario, out var userId))
                        {
                            var auditoria = new Auditoria
                            {
                                IdUsuario = userId,
                                Fecha = DateTime.UtcNow,
                                Accion = $"Cambio de estado del pedido {id} a {estado.Estado} - Stock devuelto",
                                TablaAfectada = "Pedido",
                                ValorAnterior = pedido.EstadoPedido?.Estado ?? "Desconocido",
                                ValorNuevo = estado.Estado
                            };
                            _db.Auditorias.Add(auditoria);
                        }
                    }
                    else if (estabaAnulado && !seAnula)
                    {
                        // Volver a restar stock (se está reactivando el pedido)
                        foreach (var det in pedido.Detalles)
                        {
                            var producto = await _db.Productos.FindAsync(det.IdProducto);
                            if (producto != null)
                            {
                                if (producto.Stock < det.Cantidad)
                                {
                                    await transaction.RollbackAsync();
                                    return BadRequest(new
                                    {
                                        mensaje = "No hay stock suficiente para reactivar el pedido",
                                        producto = producto.Nombre,
                                        stockDisponible = producto.Stock,
                                        cantidadNecesaria = det.Cantidad
                                    });
                                }
                                producto.Stock -= det.Cantidad;
                            }
                        }

                        // Registrar en auditoría
                        var idUsuario = User.FindFirst("id")?.Value ?? "0";
                        if (int.TryParse(idUsuario, out var userId))
                        {
                            var auditoria = new Auditoria
                            {
                                IdUsuario = userId,
                                Fecha = DateTime.UtcNow,
                                Accion = $"Cambio de estado del pedido {id} a {estado.Estado} - Stock descontado",
                                TablaAfectada = "Pedido",
                                ValorAnterior = pedido.EstadoPedido?.Estado ?? "Desconocido",
                                ValorNuevo = estado.Estado
                            };
                            _db.Auditorias.Add(auditoria);
                        }
                    }

                    pedido.IdEstadoPedido = dto.IdEstadoPedido.Value;
                }

                if (dto.IdEstadoPago.HasValue)
                {
                    var estado = await _db.EstadosPago.FindAsync(dto.IdEstadoPago.Value);
                    if (estado == null)
                        return BadRequest(new { mensaje = "Estado de pago no válido" });
                    pedido.IdEstadoPago = dto.IdEstadoPago.Value;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Estado actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = "Error al actualizar estado", error = ex.Message });
            }
        }
    }
}