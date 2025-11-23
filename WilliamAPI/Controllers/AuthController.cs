using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WilliamAPI.Data;
using WilliamAPI.DTOs;
using WilliamAPI.Helpers;
using WilliamAPI.Models;
using WilliamAPI.Services;

namespace WilliamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DBAPIContext _db;
        private readonly JwtService _jwt;

        public AuthController(DBAPIContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { mensaje = "El email ya está registrado" });

            var user = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = PasswordHelper.Hash(dto.Password),
                Rol = "Cliente"
            };

            var rolCliente = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");
            if (rolCliente != null)
            {
                user.IdRol = rolCliente.IdRol;
            }

            _db.Usuarios.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Usuario registrado",
                user = new { user.IdUsuario, user.Nombre, user.Email, user.Rol }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return Unauthorized(new { mensaje = "Credenciales inválidas" });

            var hashed = PasswordHelper.Hash(dto.Password);
            if (user.PasswordHash != hashed && user.PasswordHash != dto.Password)
                return Unauthorized(new { mensaje = "Credenciales inválidas" });

            var token = _jwt.GenerateToken(user);
            return Ok(new { token, user = new { user.IdUsuario, user.Nombre, user.Email, user.Rol } });
        }
    }
}