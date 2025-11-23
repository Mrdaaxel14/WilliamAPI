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
    public class MetodosPagoUsuarioController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public MetodosPagoUsuarioController(DBAPIContext db) => _db = db;

        private int GetUserId()
        {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpGet("mios")]
        public async Task<IActionResult> ObtenerMisMetodos()
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var metodos = await _db.MetodosPagoUsuario
                .AsNoTracking()
                .Where(m => m.IdUsuario == idUsuario)
                .Select(m => new
                {
                    m.IdMetodoPagoUsuario,
                    m.Metodo,
                    m.Titular,
                    m.Ultimos4,
                    m.Expiracion
                })
                .ToListAsync();

            return Ok(new { mensaje = "ok", response = metodos });
        }

        [HttpPost]
        public async Task<IActionResult> CrearMetodo([FromBody] MetodoPagoUsuarioDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var metodo = new MetodoPagoUsuario
            {
                IdUsuario = idUsuario,
                Metodo = dto.Metodo,
                Titular = dto.Titular,
                Ultimos4 = dto.Ultimos4,
                Expiracion = dto.Expiracion
            };

            _db.MetodosPagoUsuario.Add(metodo);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "ok",
                response = new
                {
                    metodo.IdMetodoPagoUsuario,
                    metodo.Metodo,
                    metodo.Titular,
                    metodo.Ultimos4,
                    metodo.Expiracion
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarMetodo(int id, [FromBody] MetodoPagoUsuarioDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var metodo = await _db.MetodosPagoUsuario.FirstOrDefaultAsync(m => m.IdMetodoPagoUsuario == id && m.IdUsuario == idUsuario);
            if (metodo == null) return NotFound(new { mensaje = "Método de pago no encontrado" });

            metodo.Metodo = string.IsNullOrWhiteSpace(dto.Metodo) ? metodo.Metodo : dto.Metodo;
            metodo.Titular = dto.Titular ?? metodo.Titular;
            metodo.Ultimos4 = dto.Ultimos4 ?? metodo.Ultimos4;
            metodo.Expiracion = dto.Expiracion ?? metodo.Expiracion;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "ok",
                response = new
                {
                    metodo.IdMetodoPagoUsuario,
                    metodo.Metodo,
                    metodo.Titular,
                    metodo.Ultimos4,
                    metodo.Expiracion
                }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarMetodo(int id)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var metodo = await _db.MetodosPagoUsuario.FirstOrDefaultAsync(m => m.IdMetodoPagoUsuario == id && m.IdUsuario == idUsuario);
            if (metodo == null) return NotFound(new { mensaje = "Método de pago no encontrado" });

            _db.MetodosPagoUsuario.Remove(metodo);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "ok" });
        }
    }
}