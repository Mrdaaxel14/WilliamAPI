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

        // Código secreto para registro de Admin
        private const string CODIGO_ADMIN = "WILLIAM-ADMIN-2025";

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
                Telefono = dto.Telefono,
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
                user = new { user.IdUsuario, user.Nombre, user.Email, user.Telefono, user.Rol }
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
            return Ok(new { token, user = new { user.IdUsuario, user.Nombre, user.Email, user.Telefono, user.Rol } });
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto dto)
        {
            // Validar código secreto
            if (string.IsNullOrWhiteSpace(dto.CodigoSecreto) || dto.CodigoSecreto != CODIGO_ADMIN)
                return Unauthorized(new { mensaje = "Código de autorización inválido" });

            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { mensaje = "El email ya está registrado" });

            // Validaciones para admin
            if (string.IsNullOrWhiteSpace(dto.Nombre) || dto.Nombre.Length < 2)
                return BadRequest(new { mensaje = "El nombre debe tener al menos 2 caracteres" });

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
                return BadRequest(new { mensaje = "La contraseña de admin debe tener al menos 8 caracteres" });

            // Validar complejidad de contraseña
            if (!IsStrongPassword(dto.Password))
                return BadRequest(new { mensaje = "La contraseña debe contener al menos una mayúscula, una minúscula, un número y un carácter especial" });

            var user = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Email = dto.Email.ToLower().Trim(),
                PasswordHash = PasswordHelper.Hash(dto.Password),
                Telefono = dto.Telefono?.Trim(),
                Rol = "Admin"
            };

            var rolAdmin = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Admin");
            if (rolAdmin != null)
            {
                user.IdRol = rolAdmin.IdRol;
            }

            _db.Usuarios.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                mensaje = "Administrador registrado exitosamente",
                token,
                user = new { user.IdUsuario, user.Nombre, user.Email, user.Telefono, user.Rol }
            });
        }

        private bool IsStrongPassword(string password)
        {
            var hasUpperCase = password.Any(char.IsUpper);
            var hasLowerCase = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            return password.Length >= 8 && hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }
    }
}