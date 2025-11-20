using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using WilliamAPI.Models;

namespace WilliamAPI.Data
{
    public partial class DBAPIContext : DbContext
    {
        public DBAPIContext(DbContextOptions<DBAPIContext> options) : base(options) { }

        public virtual DbSet<Usuario> Usuarios { get; set; } = null!;
        public virtual DbSet<Categoria> Categoria { get; set; } = null!;
        public virtual DbSet<Producto> Productos { get; set; } = null!;
        public virtual DbSet<Carrito> Carritos { get; set; } = null!;
        public virtual DbSet<CarritoDetalle> CarritoDetalles { get; set; } = null!;
        public virtual DbSet<Pedido> Pedidos { get; set; } = null!;
        public virtual DbSet<PedidoDetalle> PedidoDetalles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Categoria>().HasKey(c => c.IdCategoria);
            modelBuilder.Entity<Categoria>().Property(c => c.Descripcion).HasMaxLength(100);

            modelBuilder.Entity<Producto>().ToTable("Producto");
            modelBuilder.Entity<Producto>().HasKey(p => p.IdProducto);
            modelBuilder.Entity<Producto>().Property(p => p.Descripcion).HasMaxLength(100);
            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasColumnType("decimal(10,2)");
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.oCategoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .HasConstraintName("FK_Producto_Categoria");

            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Usuario>().HasKey(u => u.IdUsuario);
            modelBuilder.Entity<Usuario>().Property(u => u.Email).HasMaxLength(100);
            modelBuilder.Entity<Usuario>().Property(u => u.PasswordHash).HasMaxLength(255);

            modelBuilder.Entity<Carrito>().ToTable("Carrito");
            modelBuilder.Entity<Carrito>().HasKey(c => c.IdCarrito);
            modelBuilder.Entity<Carrito>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.IdUsuario)
                .HasConstraintName("FK_Carrito_Usuario");

            modelBuilder.Entity<CarritoDetalle>().ToTable("CarritoDetalle");
            modelBuilder.Entity<CarritoDetalle>().HasKey(d => d.IdCarritoDetalle);
            modelBuilder.Entity<CarritoDetalle>()
                .HasOne(d => d.Carrito)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.IdCarrito)
                .HasConstraintName("FK_CarritoDetalle_Carrito");
            modelBuilder.Entity<CarritoDetalle>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .HasConstraintName("FK_CarritoDetalle_Producto");

            modelBuilder.Entity<Pedido>().ToTable("Pedido");
            modelBuilder.Entity<Pedido>().HasKey(p => p.IdPedido);
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.IdUsuario)
                .HasConstraintName("FK_Pedido_Usuario");

            modelBuilder.Entity<PedidoDetalle>().ToTable("PedidoDetalle");
            modelBuilder.Entity<PedidoDetalle>().HasKey(d => d.IdPedidoDetalle);
            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdPedido)
                .HasConstraintName("FK_PedidoDetalle_Pedido");
            modelBuilder.Entity<PedidoDetalle>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .HasConstraintName("FK_PedidoDetalle_Producto");
        }
    }
}
