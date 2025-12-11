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
                    Nombre = p.Nombre,
                    Marca = p.Marca,
                    IdCategoria = p.IdCategoria,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    EnStock = p.Stock > 0,
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
                    Nombre = p.Nombre,
                    Marca = p.Marca,
                    IdCategoria = p.IdCategoria,
                    CategoriaNombre = p.Categoria != null ? p.Categoria.Descripcion : null,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    EnStock = p.Stock > 0,
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
                Nombre = dto.Nombre,
                Marca = dto.Marca,
                IdCategoria = dto.IdCategoria,
                Precio = dto.Precio,
                Stock = dto.Stock
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
            p.Nombre = dto.Nombre ?? p.Nombre;
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

        // GET /api/producto/buscar?q=remera&idCategoria=1&precioMin=1000&precioMax=50000&pagina=1&porPagina=20
        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar(
            string? q,
            int? idCategoria,
            decimal? precioMin,
            decimal? precioMax,
            string? marca,
            string? ordenarPor,      // "precio_asc", "precio_desc", "nombre_asc", "nombre_desc"
            int pagina = 1,
            int porPagina = 20)
        {
            // Validar parámetros de paginación
            if (pagina < 1) pagina = 1;
            if (porPagina < 1) porPagina = 20;
            if (porPagina > 100) porPagina = 100; // Máximo 100 items por página

            var query = _db.Productos
                .Include(p => p.Imagenes)
                .Include(p => p.Categoria)
                .AsNoTracking()
                .AsQueryable();

            // Filtro por texto (busca en descripción y marca)
            if (!string.IsNullOrWhiteSpace(q))
            {
                var busqueda = q.ToLower().Trim();
                query = query.Where(p =>
                    p.Descripcion.ToLower().Contains(busqueda) ||
                    (p.Marca != null && p.Marca.ToLower().Contains(busqueda)) ||
                    (p.CodigoBarra != null && p.CodigoBarra.Contains(busqueda))
                );
            }

            // Filtro por categoría
            if (idCategoria.HasValue)
            {
                query = query.Where(p => p.IdCategoria == idCategoria.Value);
            }

            // Filtro por marca
            if (!string.IsNullOrWhiteSpace(marca))
            {
                query = query.Where(p => p.Marca != null && p.Marca.ToLower() == marca.ToLower().Trim());
            }

            // Filtro por rango de precio
            if (precioMin.HasValue)
            {
                query = query.Where(p => p.Precio >= precioMin.Value);
            }
            if (precioMax.HasValue)
            {
                query = query.Where(p => p.Precio <= precioMax.Value);
            }

            // Ordenamiento
            query = ordenarPor?.ToLower() switch
            {
                "precio_asc" => query.OrderBy(p => p.Precio),
                "precio_desc" => query.OrderByDescending(p => p.Precio),
                "nombre_asc" => query.OrderBy(p => p.Descripcion),
                "nombre_desc" => query.OrderByDescending(p => p.Descripcion),
                _ => query.OrderBy(p => p.IdProducto) // Por defecto
            };

            // Contar total antes de paginar
            var totalItems = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling((double)totalItems / porPagina);

            // Aplicar paginación
            var productos = await query
                .Skip((pagina - 1) * porPagina)
                .Take(porPagina)
                .Select(p => new ProductoListaDto
                {
                    IdProducto = p.IdProducto,
                    CodigoBarra = p.CodigoBarra,
                    Descripcion = p.Descripcion,
                    Nombre = p.Nombre,
                    Marca = p.Marca,
                    IdCategoria = p.IdCategoria,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    EnStock = p.Stock > 0,
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

            var resultado = new BuscarProductoResultDto
            {
                TotalItems = totalItems,
                TotalPaginas = totalPaginas,
                PaginaActual = pagina,
                ItemsPorPagina = porPagina,
                Productos = productos
            };

            return Ok(new { mensaje = "ok", response = resultado });
        }

        // GET /api/producto/marcas (obtener lista de marcas disponibles)
        [HttpGet("marcas")]
        public async Task<IActionResult> ObtenerMarcas()
        {
            var marcas = await _db.Productos
                .AsNoTracking()
                .Where(p => p.Marca != null && p.Marca != "")
                .Select(p => p.Marca)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            return Ok(new { mensaje = "ok", response = marcas });
        }

        // GET /api/producto/{id}/stock (verificar stock de un producto)
        [HttpGet("{id}/stock")]
        public async Task<IActionResult> VerificarStock(int id)
        {
            var producto = await _db.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            var stock = await _db.Stocks
                .AsNoTracking()
                .Include(s => s.EstadoStock)
                .FirstOrDefaultAsync(s => s.IdProducto == id);

            return Ok(new
            {
                mensaje = "ok",
                response = new
                {
                    IdProducto = id,
                    Cantidad = stock?.Cantidad ?? 0,
                    Estado = stock?.EstadoStock?.Estado ?? "Sin stock",
                    Disponible = (stock?.Cantidad ?? 0) > 0
                }
            });
        }

        // PUT /api/producto/{id}/stock (Admin - actualizar stock manualmente)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}/stock")]
        public async Task<IActionResult> ActualizarStock(int id, [FromBody] ActualizarStockDto dto)
        {
            var producto = await _db.Productos.FindAsync(id);
            if (producto == null)
                return NotFound(new { mensaje = "Producto no encontrado" });

            // Validar que al menos uno de los campos esté presente
            if (!dto.CantidadAbsoluta.HasValue && !dto.CantidadAjuste.HasValue)
            {
                return BadRequest(new { mensaje = "Debe proporcionar CantidadAbsoluta o CantidadAjuste" });
            }

            // No permitir ambos campos a la vez
            if (dto.CantidadAbsoluta.HasValue && dto.CantidadAjuste.HasValue)
            {
                return BadRequest(new { mensaje = "Solo puede usar CantidadAbsoluta o CantidadAjuste, no ambos" });
            }

            int stockAnterior = producto.Stock;

            // Actualizar stock según el tipo de operación
            if (dto.CantidadAbsoluta.HasValue)
            {
                // Establecer stock directamente
                if (dto.CantidadAbsoluta.Value < 0)
                {
                    return BadRequest(new { mensaje = "El stock no puede ser negativo" });
                }
                producto.Stock = dto.CantidadAbsoluta.Value;
            }
            else if (dto.CantidadAjuste.HasValue)
            {
                // Ajustar stock (agregar o restar)
                int nuevoStock = producto.Stock + dto.CantidadAjuste.Value;
                if (nuevoStock < 0)
                {
                    return BadRequest(new { mensaje = "El ajuste resultaría en stock negativo" });
                }
                producto.Stock = nuevoStock;
            }

            await _db.SaveChangesAsync();

            // Registrar en auditoría si existe tabla
            try
            {
                var userId = User.FindFirst("id")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userId, out var idUsuario))
                {
                    var auditoria = new Auditoria
                    {
                        IdUsuario = idUsuario,
                        Fecha = DateTime.UtcNow,
                        Accion = $"Actualización manual de stock: {stockAnterior} → {producto.Stock}",
                        TablaAfectada = "Producto",
                        ValorAnterior = stockAnterior.ToString(),
                        ValorNuevo = producto.Stock.ToString()
                    };
                    _db.Auditorias.Add(auditoria);
                    await _db.SaveChangesAsync();
                }
            }
            catch
            {
                // Si falla la auditoría, continuar (no es crítico)
            }

            return Ok(new
            {
                mensaje = "Stock actualizado exitosamente",
                stockAnterior = stockAnterior,
                stockNuevo = producto.Stock,
                response = new
                {
                    producto.IdProducto,
                    producto.Nombre,
                    producto.Stock,
                    EnStock = producto.Stock > 0
                }
            });
        }
    }
}