using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WilliamAPI.Data;
using WilliamAPI.DTOs;
using WilliamAPI.Helpers;

namespace WilliamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Cliente")]
    public class PerfilController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public PerfilController(DBAPIContext db) => _db = db;

        private int GetUserId()
        {
            var idClaim = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idClaim, out var id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var usuario = await _db.Usuarios
                .AsNoTracking()
                .Where(u => u.IdUsuario == idUsuario)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Email,
                    u.Telefono,
                    u.Rol,
                    u.IdRol,
                    u.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            return Ok(new { mensaje = "ok", response = usuario });
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarPerfil([FromBody] UpdatePerfilDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var usuario = await _db.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                usuario.Nombre = dto.Nombre;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
            {
                var emailExistente = await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.IdUsuario != idUsuario);
                if (emailExistente)
                    return BadRequest(new { mensaje = "El email ya está registrado" });

                usuario.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Telefono))
                usuario.Telefono = dto.Telefono;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "ok",
                response = new { usuario.IdUsuario, usuario.Nombre, usuario.Email, usuario.Telefono, usuario.Rol, usuario.IdRol, usuario.FechaRegistro }
            });
        }

        [HttpPut("password")]
        public async Task<IActionResult> CambiarPassword([FromBody] ChangePasswordDto dto)
        {
            var idUsuario = GetUserId();
            if (idUsuario == 0) return Unauthorized();

            var usuario = await _db.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            var hashActual = PasswordHelper.Hash(dto.PasswordActual);
            if (usuario.PasswordHash != hashActual && usuario.PasswordHash != dto.PasswordActual)
                return Unauthorized(new { mensaje = "Contraseña actual incorrecta" });

            usuario.PasswordHash = PasswordHelper.Hash(dto.NuevoPassword);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada" });
        }
    }
}