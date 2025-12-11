using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using WilliamAPI.Models;

namespace WilliamAPI.Data
{
    public partial class DBAPIContext : DbContext
    {
        public DBAPIContext(DbContextOptions<DBAPIContext> options) : base(options) { }

        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;
        public virtual DbSet<Rol> Roles { get; set; } = null!;
        public virtual DbSet<Permiso> Permisos { get; set; } = null!;
        public virtual DbSet<Categoria> Categorias { get; set; } = null!;
        public virtual DbSet<Producto> Productos { get; set; } = null!;
        public virtual DbSet<Carrito> Carritos { get; set; } = null!;
        public virtual DbSet<CarritoDetalle> CarritoDetalles { get; set; } = null!;
        public virtual DbSet<Pedido> Pedidos { get; set; } = null!;
        public virtual DbSet<PedidoDetalle> PedidoDetalles { get; set; } = null!;
        public virtual DbSet<DireccionUsuario> DireccionesUsuario { get; set; } = null!;
        public virtual DbSet<MetodoPago> MetodosPago { get; set; } = null!;
        public virtual DbSet<MetodoPagoUsuario> MetodosPagoUsuario { get; set; } = null!;
        public virtual DbSet<EstadoPedido> EstadosPedido { get; set; } = null!;
        public virtual DbSet<EstadoPago> EstadosPago { get; set; } = null!;
        public virtual DbSet<EstadoStock> EstadosStock { get; set; } = null!;
        public virtual DbSet<Stock> Stocks { get; set; } = null!;
        public virtual DbSet<ImagenProducto> ImagenesProducto { get; set; } = null!;
        public virtual DbSet<Compra> Compras { get; set; } = null!;
        public virtual DbSet<CompraDetalle> CompraDetalles { get; set; } = null!;
        public virtual DbSet<Auditoria> Auditorias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Telefono).HasMaxLength(20);
                entity.Property(e => e.Rol).HasMaxLength(20).HasDefaultValue("Cliente");
                entity.Property(e => e.FechaRegistro).HasColumnType("datetime").HasDefaultValueSql("getdate()");
                entity.HasIndex(e => e.Email).IsUnique();

                entity.HasOne(e => e.RolNavigation)
                      .WithMany(r => r.Usuarios)
                      .HasForeignKey(e => e.IdRol);
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.IdRol);
                entity.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
                entity.HasIndex(r => r.Nombre).IsUnique();
            });

            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.ToTable("Permisos");
                entity.HasKey(p => p.IdPermiso);
                entity.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
                entity.HasIndex(p => p.Nombre).IsUnique();
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categoria");
                entity.HasKey(c => c.IdCategoria);
                entity.Property(c => c.Descripcion).HasMaxLength(100).IsRequired();
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Producto");
                entity.HasKey(p => p.IdProducto);
                entity.Property(p => p.CodigoBarra).HasMaxLength(30);
                entity.Property(p => p.Descripcion).HasMaxLength(100).IsRequired();
                entity.Property(p => p.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(p => p.Marca).HasMaxLength(50);
                entity.Property(p => p.Precio).HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(p => p.Stock).HasDefaultValue(0);

                entity.HasOne(p => p.Categoria)
                      .WithMany(c => c.Productos)
                      .HasForeignKey(p => p.IdCategoria)
                      .HasConstraintName("FK_Producto_Categoria");
            });

            modelBuilder.Entity<Carrito>(entity =>
            {
                entity.ToTable("Carrito");
                entity.HasKey(c => c.IdCarrito);

                entity.HasOne(c => c.Usuario)
                      .WithMany(u => u.Carritos)
                      .HasForeignKey(c => c.IdUsuario)
                      .HasConstraintName("FK_Carrito_Usuario");
            });

            modelBuilder.Entity<CarritoDetalle>(entity =>
            {
                entity.ToTable("CarritoDetalle");
                entity.HasKey(cd => cd.IdCarritoDetalle);
                entity.Property(cd => cd.Cantidad).IsRequired();

                entity.HasOne(cd => cd.Carrito)
                      .WithMany(c => c.Detalles)
                      .HasForeignKey(cd => cd.IdCarrito)
                      .HasConstraintName("FK_CarritoDetalle_Carrito");

                entity.HasOne(cd => cd.Producto)
                      .WithMany(p => p.CarritoDetalles)
                      .HasForeignKey(cd => cd.IdProducto)
                      .HasConstraintName("FK_CarritoDetalle_Producto");
            });

            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.ToTable("Pedido");
                entity.HasKey(p => p.IdPedido);
                entity.Property(p => p.Fecha).HasColumnType("datetime").HasDefaultValueSql("getdate()");
                entity.Property(p => p.Total).HasColumnType("decimal(10,2)");

                entity.HasOne(p => p.Usuario)
                      .WithMany(u => u.Pedidos)
                      .HasForeignKey(p => p.IdUsuario)
                      .HasConstraintName("FK_Pedido_Usuario");

                entity.HasOne(p => p.MetodoPago)
                      .WithMany(m => m.Pedidos)
                      .HasForeignKey(p => p.IdMetodoPago)
                      .HasConstraintName("FK_Pedido_MetodoPago");

                entity.HasOne(p => p.EstadoPedido)
                      .WithMany(e => e.Pedidos)
                      .HasForeignKey(p => p.IdEstadoPedido)
                      .HasConstraintName("FK_Pedido_EstadoPedido");

                entity.HasOne(p => p.EstadoPago)
                      .WithMany(e => e.Pedidos)
                      .HasForeignKey(p => p.IdEstadoPago)
                      .HasConstraintName("FK_Pedido_EstadoPago");

                // ← NUEVO: Relación con Dirección
                entity.HasOne(p => p.Direccion)
                      .WithMany()
                      .HasForeignKey(p => p.IdDireccion)
                      .HasConstraintName("FK_Pedido_Direccion");
            });

            modelBuilder.Entity<PedidoDetalle>(entity =>
            {
                entity.ToTable("PedidoDetalle");
                entity.HasKey(pd => pd.IdPedidoDetalle);
                entity.Property(pd => pd.Cantidad).IsRequired();
                entity.Property(pd => pd.PrecioUnitario).HasColumnType("decimal(10,2)");

                entity.HasOne(pd => pd.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(pd => pd.IdPedido)
                      .HasConstraintName("FK_PedidoDetalle_Pedido");

                entity.HasOne(pd => pd.Producto)
                      .WithMany(p => p.PedidoDetalles)
                      .HasForeignKey(pd => pd.IdProducto)
                      .HasConstraintName("FK_PedidoDetalle_Producto");
            });

            modelBuilder.Entity<DireccionUsuario>(entity =>
            {
                entity.ToTable("DireccionesUsuario");
                entity.HasKey(d => d.IdDireccion);
                entity.Property(d => d.Provincia).HasMaxLength(100);
                entity.Property(d => d.Ciudad).HasMaxLength(100);
                entity.Property(d => d.Calle).HasMaxLength(150);
                entity.Property(d => d.Numero).HasMaxLength(20);
                entity.Property(d => d.CodigoPostal).HasMaxLength(20);

                entity.HasOne(d => d.Usuario)
                                  .WithMany(u => u.Direcciones)
                                  .HasForeignKey(d => d.IdUsuario)
                                  .HasConstraintName("FK_Direcciones_Usuario");
            });

            modelBuilder.Entity<MetodoPago>(entity =>
            {
                entity.ToTable("MetodoPago");
                entity.HasKey(m => m.IdMetodoPago);
                entity.Property(m => m.Metodo).HasMaxLength(50).IsRequired();
            });

            modelBuilder.Entity<MetodoPagoUsuario>(entity =>
            {
                entity.ToTable("MetodoPagoUsuario");
    entity.HasKey(m => m.IdMetodoPagoUsuario);
    entity.Property(m => m.Metodo).HasMaxLength(50).IsRequired();
    entity.Property(m => m.Titular).HasMaxLength(150);
    entity.Property(m => m.Ultimos4).HasMaxLength(4);
    entity.Property(m => m.Expiracion).HasMaxLength(7);

    entity.HasOne(m => m.Usuario)
          .WithMany(u => u.MetodosPago)
          .HasForeignKey(m => m.IdUsuario)
          .HasConstraintName("FK_MetodoPagoUsuario_Usuario");
});

modelBuilder.Entity<EstadoPedido>(entity =>
{
    entity.ToTable("EstadoPedido");
    entity.HasKey(e => e.IdEstadoPedido);
    entity.Property(e => e.Estado).HasMaxLength(50).IsRequired();
    entity.HasIndex(e => e.Estado).IsUnique();
});

modelBuilder.Entity<EstadoPago>(entity =>
{
    entity.ToTable("EstadoPago");
    entity.HasKey(e => e.IdEstadoPago);
    entity.Property(e => e.Estado).HasMaxLength(50).IsRequired();
    entity.HasIndex(e => e.Estado).IsUnique();
});

modelBuilder.Entity<EstadoStock>(entity =>
{
    entity.ToTable("EstadoStock");
    entity.HasKey(e => e.IdEstadoStock);
    entity.Property(e => e.Estado).HasMaxLength(50).IsRequired();
    entity.HasIndex(e => e.Estado).IsUnique();
});

modelBuilder.Entity<Stock>(entity =>
{
                entity.ToTable("Stock");
                entity.HasKey(s => s.IdStock);
                entity.Property(s => s.Cantidad).HasDefaultValue(0);

                entity.HasOne(s => s.Producto)
                      .WithMany(p => p.Stocks)
                      .HasForeignKey(s => s.IdProducto)
                      .HasConstraintName("FK_Stock_Producto");

                entity.HasOne(s => s.EstadoStock)
                      .WithMany(e => e.Stocks)
                      .HasForeignKey(s => s.IdEstadoStock)
                      .HasConstraintName("FK_Stock_EstadoStock");
            });

            // Agregar esta sección en OnModelCreating, reemplazando la configuración actual de ImagenProducto:

            modelBuilder.Entity<ImagenProducto>(entity =>
            {
                entity.ToTable("ImagenProducto");
                entity.HasKey(i => i.IdImagen);
                entity.Property(i => i.UrlImagen).HasMaxLength(500).IsRequired();
                entity.Property(i => i.Orden).HasDefaultValue(0);
                entity.Property(i => i.EsPrincipal).HasDefaultValue(false);

                entity.HasOne(i => i.Producto)
                      .WithMany(p => p.Imagenes)
                      .HasForeignKey(i => i.IdProducto)
                      .HasConstraintName("FK_Producto_Imagen");

                // Índice para optimizar consultas
                entity.HasIndex(i => new { i.IdProducto, i.Orden })
                      .HasDatabaseName("IX_ImagenProducto_IdProducto_Orden");
            });

            modelBuilder.Entity<Compra>(entity =>
            {
                entity.ToTable("Compra");
                entity.HasKey(c => c.IdCompra);
                entity.Property(c => c.Fecha).HasColumnType("datetime").HasDefaultValueSql("getdate()");
                entity.Property(c => c.Total).HasColumnType("decimal(10,2)");

                entity.HasOne(c => c.Usuario)
                      .WithMany(u => u.Compras)
                      .HasForeignKey(c => c.IdUsuario)
                      .HasConstraintName("FK_Compra_Usuario");
            });

            modelBuilder.Entity<CompraDetalle>(entity =>
            {
                entity.ToTable("CompraDetalle");
                entity.HasKey(c => c.IdCompraDetalle);
                entity.Property(c => c.CostoUnitario).HasColumnType("decimal(10,2)");

                entity.HasOne(c => c.Compra)
                      .WithMany(co => co.Detalles)
                      .HasForeignKey(c => c.IdCompra)
                      .HasConstraintName("FK_CompraDetalle_Compra");

                entity.HasOne(c => c.Producto)
                      .WithMany(p => p.CompraDetalles)
                      .HasForeignKey(c => c.IdProducto)
                      .HasConstraintName("FK_CompraDetalle_Producto");
            });

            modelBuilder.Entity<Auditoria>(entity =>
            {
                entity.ToTable("Auditoria");
                entity.HasKey(a => a.IdAuditoria);
                entity.Property(a => a.Fecha).HasColumnType("datetime").HasDefaultValueSql("getdate()");
                entity.Property(a => a.Accion).HasMaxLength(200);
                entity.Property(a => a.TablaAfectada).HasMaxLength(100);

                entity.HasOne(a => a.Usuario)
                      .WithMany(u => u.Auditorias)
                      .HasForeignKey(a => a.IdUsuario)
                      .HasConstraintName("FK_Auditoria_Usuario");
            });
        }
    }
}