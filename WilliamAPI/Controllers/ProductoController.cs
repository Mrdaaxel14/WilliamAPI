using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WilliamAPI.Data;
using WilliamAPI.Models;

namespace WilliamAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public ProductoController(DBAPIContext db) => _db = db;

        // GET /api/producto/lista
        [HttpGet("lista")]
        public async Task<IActionResult> Lista()
        {
            var lista = await _db.Productos.AsNoTracking().ToListAsync();
            return Ok(lista);
        }

        // GET /api/producto/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var p = await _db.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.IdProducto == id);
            if (p == null) return NotFound(new { mensaje = "Producto no encontrado" });
            return Ok(p);
        }

        // POST /api/producto  (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Producto producto)
        {
            _db.Productos.Add(producto);
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok", id = producto.IdProducto });
        }

        // PUT /api/producto/{id}  (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Producto modelo)
        {
            var p = await _db.Productos.FindAsync(id);
            if (p == null) return NotFound(new { mensaje = "Producto no encontrado" });

            p.CodigoBarra = modelo.CodigoBarra ?? p.CodigoBarra;
            p.Descripcion = modelo.Descripcion ?? p.Descripcion;
            p.Marca = modelo.Marca ?? p.Marca;
            p.IdCategoria = modelo.IdCategoria ?? p.IdCategoria;
            p.Precio = modelo.Precio != 0 ? modelo.Precio : p.Precio;

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        // DELETE /api/producto/{id}  (Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var p = await _db.Productos.FindAsync(id);
            if (p == null) return NotFound(new { mensaje = "Producto no encontrado" });
            _db.Productos.Remove(p);
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }
    }
}
