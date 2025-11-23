using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WilliamAPI.Data;
using WilliamAPI.DTOs;
using WilliamAPI.Models;

namespace WilliamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Cliente")]
    public class CarritoController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public CarritoController(DBAPIContext db) => _db = db;

        // Helper: obtener idUsuario desde token
        private int GetUserId()
        {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        // POST api/carrito/agregar
        [HttpPost("agregar")]
        public async Task<IActionResult> Agregar([FromBody] AddCarritoDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            if (dto.Cantidad <= 0)
                return BadRequest(new { mensaje = "La cantidad debe ser mayor a cero" });

            var producto = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.IdProducto == dto.IdProducto);
            if (producto == null) return NotFound(new { mensaje = "Producto no encontrado" });

            var carrito = await _db.Carritos.Include(c => c.Detalles)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null)
            {
                carrito = new Carrito { IdUsuario = idUsuario };
                _db.Carritos.Add(carrito);
                await _db.SaveChangesAsync();
            }

            var detalle = carrito.Detalles.FirstOrDefault(d => d.IdProducto == dto.IdProducto);
            if (detalle == null)
            {
                detalle = new CarritoDetalle
                {
                    IdCarrito = carrito.IdCarrito,
                    IdProducto = dto.IdProducto,
                    Cantidad = dto.Cantidad
                };
                _db.CarritoDetalles.Add(detalle);
            }
            else
            {
                detalle.Cantidad += dto.Cantidad;
            }

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        // GET api/carrito/mis-items
        [HttpGet("mis-items")]
        public async Task<IActionResult> MisItems()
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var carrito = await _db.Carritos
                .Include(c => c.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.IdUsuario == idUsuario);

            if (carrito == null) return Ok(new { mensaje = "ok", response = new List<object>() });

            var items = carrito.Detalles.Select(d => new
            {
                d.IdCarritoDetalle,
                Producto = d.Producto == null ? null : new { d.Producto.IdProducto, d.Producto.Descripcion, d.Producto.Precio, d.Producto.Marca },
                d.Cantidad
            }).ToList();

            return Ok(new { mensaje = "ok", response = items });
        }

        // DELETE api/carrito/eliminar/{idDetalle}
        [HttpDelete("eliminar/{idDetalle:int}")]
        public async Task<IActionResult> EliminarDetalle(int idDetalle)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var detalle = await _db.CarritoDetalles
                .Include(d => d.Carrito)
                .FirstOrDefaultAsync(d => d.IdCarritoDetalle == idDetalle && d.Carrito!.IdUsuario == idUsuario);

            if (detalle == null) return NotFound(new { mensaje = "Detalle no encontrado" });

            _db.CarritoDetalles.Remove(detalle);
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }
    }
}