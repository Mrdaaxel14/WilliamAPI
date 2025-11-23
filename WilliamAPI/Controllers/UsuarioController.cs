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
    [Authorize(Roles = "Admin")]
    public class UsuarioController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public UsuarioController(DBAPIContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _db.Usuarios
                .AsNoTracking()
                .Include(u => u.RolNavigation)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Email,
                    u.Rol,
                    u.IdRol,
                    RolNombre = u.RolNavigation != null ? u.RolNavigation.Nombre : u.Rol,
                    u.FechaRegistro
                })
                .ToListAsync();

            return Ok(new { mensaje = "ok", response = usuarios });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _db.Usuarios
                .AsNoTracking()
                .Include(u => u.RolNavigation)
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Email,
                    u.Rol,
                    u.IdRol,
                    RolNombre = u.RolNavigation != null ? u.RolNavigation.Nombre : u.Rol,
                    u.FechaRegistro
                })
                .FirstOrDefaultAsync();

            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });
            return Ok(new { mensaje = "ok", response = usuario });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] UpdateUsuarioDto dto)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                usuario.Nombre = dto.Nombre;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
            {
                var emailExistente = await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.IdUsuario != id);
                if (emailExistente)
                    return BadRequest(new { mensaje = "El email ya está registrado" });

                usuario.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Rol))
                usuario.Rol = dto.Rol;

            if (dto.IdRol.HasValue)
            {
                var rol = await _db.Roles.FindAsync(dto.IdRol.Value);
                if (rol == null)
                    return BadRequest(new { mensaje = "Rol inválido" });

                usuario.IdRol = rol.IdRol;
                usuario.Rol = rol.Nombre;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "ok",
                response = new { usuario.IdUsuario, usuario.Nombre, usuario.Email, usuario.Rol, usuario.IdRol, usuario.FechaRegistro }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado" });

            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "ok" });
        }
    }
}