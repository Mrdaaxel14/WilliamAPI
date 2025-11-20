using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WilliamAPI.Data;
using WilliamAPI.Models;

namespace WilliamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public CategoriaController(DBAPIContext db) => _db = db;

        [HttpGet("lista")]
        public async Task<IActionResult> Lista()
        {
            var categorias = await _db.Categoria.ToListAsync();
            return Ok(new { mensaje = "ok", response = categorias });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] Categoria cat)
        {
            _db.Categoria.Add(cat);
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("editar/{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Categoria cat)
        {
            var o = await _db.Categoria.FindAsync(id);
            if (o == null) return NotFound("Categoría no encontrada");
            o.Descripcion = cat.Descripcion ?? o.Descripcion;
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("eliminar/{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var o = await _db.Categoria.FindAsync(id);
            if (o == null) return NotFound("Categoría no encontrada");
            _db.Categoria.Remove(o);
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }
    }
}
