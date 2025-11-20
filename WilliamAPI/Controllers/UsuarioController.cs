using WilliamAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WilliamAPI.Data;

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
                .Select(u => new { u.IdUsuario, u.Nombre, u.Email, u.Rol, u.FechaRegistro })
                .ToListAsync();

            return Ok(new { mensaje = "ok", response = usuarios });
        }
    }
}
