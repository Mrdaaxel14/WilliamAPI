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
    [Authorize(Roles = "Cliente")]
    public class DireccionesController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public DireccionesController(DBAPIContext db) => _db = db;

        private int GetUserId()
        {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpGet("mias")]
        public async Task<IActionResult> ObtenerMisDirecciones()
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var direcciones = await _db.DireccionesUsuario
                .AsNoTracking()
                .Where(d => d.IdUsuario == idUsuario)
                .Select(d => new
                {
                    d.IdDireccion,
                    d.Provincia,
                    d.Ciudad,
                    d.Calle,
                    d.Numero,
                    d.CodigoPostal
                })
                .ToListAsync();

            return Ok(new { mensaje = "ok", response = direcciones });
        }

        [HttpPost]
        public async Task<IActionResult> CrearDireccion([FromBody] DireccionDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var direccion = new DireccionUsuario
            {
                IdUsuario = idUsuario,
                Provincia = dto.Provincia,
                Ciudad = dto.Ciudad,
                Calle = dto.Calle,
                Numero = dto.Numero,
                CodigoPostal = dto.CodigoPostal
            };

            _db.DireccionesUsuario.Add(direccion);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "ok",
                response = new
                {
                    direccion.IdDireccion,
                    direccion.Provincia,
                    direccion.Ciudad,
                    direccion.Calle,
                    direccion.Numero,
                    direccion.CodigoPostal
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarDireccion(int id, [FromBody] DireccionDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var direccion = await _db.DireccionesUsuario.FirstOrDefaultAsync(d => d.IdDireccion == id && d.IdUsuario == idUsuario);
            if (direccion == null) return NotFound(new { mensaje = "Dirección no encontrada" });

            direccion.Provincia = dto.Provincia ?? direccion.Provincia;
            direccion.Ciudad = dto.Ciudad ?? direccion.Ciudad;
            direccion.Calle = dto.Calle ?? direccion.Calle;
            direccion.Numero = dto.Numero ?? direccion.Numero;
            direccion.CodigoPostal = dto.CodigoPostal ?? direccion.CodigoPostal;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "ok",
                response = new
                {
                    direccion.IdDireccion,
                    direccion.Provincia,
                    direccion.Ciudad,
                    direccion.Calle,
                    direccion.Numero,
                    direccion.CodigoPostal
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarDireccion(int id)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var direccion = await _db.DireccionesUsuario.FirstOrDefaultAsync(d => d.IdDireccion == id && d.IdUsuario == idUsuario);
            if (direccion == null) return NotFound(new { mensaje = "Dirección no encontrada" });

            _db.DireccionesUsuario.Remove(direccion);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "ok" });
        }
    }
}