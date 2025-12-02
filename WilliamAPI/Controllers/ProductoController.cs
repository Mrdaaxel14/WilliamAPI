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
    public class ProductoController : ControllerBase
    {
        private readonly DBAPIContext _db;
        public ProductoController(DBAPIContext db) => _db = db;

        // GET /api/producto/lista
        [HttpGet("lista")]
        public async Task<IActionResult> Lista()
        {
            var lista = await _db.Productos
                .Include(p => p.Imagenes)
                .AsNoTracking()
                .Select(p => new ProductoListaDto
                {
                    IdProducto = p.IdProducto,
                    CodigoBarra = p.CodigoBarra,
                    Descripcion = p.Descripcion,
                    Marca = p.Marca,
                    IdCategoria = p.IdCategoria,
                    Precio = p.Precio,
                    ImagenPrincipal = p.Imagenes
                        .Where(i => i.EsPrincipal)
                        .Select(i => i.UrlImagen)
                        .FirstOrDefault()
                        ?? p.Imagenes
                            .OrderBy(i => i.Orden)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { mensaje = "ok", response = lista });
        }

        // GET /api/producto/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var producto = await _db.Productos
                .Include(p => p.Imagenes)
                .Include(p => p.Categoria)
                .AsNoTracking()
                .Where(p => p.IdProducto == id)
                .Select(p => new ProductoDetalleDto
                {
                    IdProducto = p.IdProducto,
                    CodigoBarra = p.CodigoBarra,
                    Descripcion = p.Descripcion,
                    Marca = p.Marca,
                    IdCategoria = p.IdCategoria,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Descripcion : null,
                    Precio = p.Precio,
                    ImagenPrincipal = p.Imagenes
                        .Where(i => i.EsPrincipal)
                        .Select(i => i.UrlImagen)
                        .FirstOrDefault()
                        ?? p.Imagenes
                            .OrderBy(i => i.Orden)
                            .Select(i => i.UrlImagen)
                            .FirstOrDefault(),
                    Galeria = p.Imagenes
                        .OrderBy(i => i.Orden)
                        .Select(i => i.UrlImagen)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            return Ok(new { mensaje = "ok", response = producto });
        }

        // POST /api/producto (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ProductoCreateUpdateDto dto)
        {
            var producto = new Producto
            {
                CodigoBarra = dto.CodigoBarra,
                Descripcion = dto.Descripcion,
                Marca = dto.Marca,
                IdCategoria = dto.IdCategoria,
                Precio = dto.Precio
            };

            _db.Productos.Add(producto);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "ok", id = producto.IdProducto });
        }

        // PUT /api/producto/{id} (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] ProductoCreateUpdateDto dto)
        {
            var p = await _db.Productos.FindAsync(id);
            if (p == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            p.CodigoBarra = dto.CodigoBarra ?? p.CodigoBarra;
            p.Descripcion = dto.Descripcion ?? p.Descripcion;
            p.Marca = dto.Marca ?? p.Marca;
            p.IdCategoria = dto.IdCategoria ?? p.IdCategoria;
            p.Precio = dto.Precio != 0 ? dto.Precio : p.Precio;

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        // DELETE /api/producto/{id} (Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var p = await _db.Productos.FindAsync(id);
            if (p == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            _db.Productos.Remove(p);
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        // POST /api/producto/{id}/imagenes (Admin) - Agregar imágenes
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/imagenes")]
        public async Task<IActionResult> AgregarImagenes(int id, [FromBody] List<ImagenProductoDto> imagenes)
        {
            var producto = await _db.Productos
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            // Si se marca una nueva como principal, desmarcar las anteriores
            if (imagenes.Any(i => i.EsPrincipal))
            {
                foreach (var img in producto.Imagenes)
                {
                    img.EsPrincipal = false;
                }
            }

            foreach (var dto in imagenes)
            {
                var nuevaImagen = new ImagenProducto
                {
                    IdProducto = id,
                    UrlImagen = dto.UrlImagen,
                    EsPrincipal = dto.EsPrincipal,
                    Orden = dto.Orden
                };

                _db.ImagenesProducto.Add(nuevaImagen);
            }

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        // PUT /api/producto/{id}/imagenes/{idImagen}/principal (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/imagenes/{idImagen:int}/principal")]
        public async Task<IActionResult> MarcarImagenPrincipal(int id, int idImagen)
        {
            var producto = await _db.Productos
                .Include(p => p.Imagenes)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            var imagen = producto.Imagenes.FirstOrDefault(i => i.IdImagen == idImagen);
            if (imagen == null)
                return NotFound(new { mensaje = "Imagen no encontrada" });

            // Desmarcar todas como principal
            foreach (var img in producto.Imagenes)
            {
                img.EsPrincipal = false;
            }

            // Marcar la seleccionada
            imagen.EsPrincipal = true;
            imagen.Orden = 0;

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "ok" });
        }

        // DELETE /api/producto/{id}/imagenes/{idImagen} (Admin)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}/imagenes/{idImagen:int}")]
        public async Task<IActionResult> EliminarImagen(int id, int idImagen)
        {
            var imagen = await _db.ImagenesProducto
                .FirstOrDefaultAsync(i => i.IdImagen == idImagen && i.IdProducto == id);

            if (imagen == null)
                return NotFound(new { mensaje = "Imagen no encontrada" });

            _db.ImagenesProducto.Remove(imagen);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "ok" });
        }

        // GET /api/producto/{id}/imagenes
        [HttpGet("{id:int}/imagenes")]
        public async Task<IActionResult> ObtenerImagenes(int id)
        {
            var imagenes = await _db.ImagenesProducto
                .AsNoTracking()
                .Where(i => i.IdProducto == id)
                .OrderBy(i => i.Orden)
                .Select(i => new
                {
                    i.IdImagen,
                    i.UrlImagen,
                    i.EsPrincipal,
                    i.Orden
                })
                .ToListAsync();

            return Ok(new { mensaje = "ok", response = imagenes });
        }
    }
}