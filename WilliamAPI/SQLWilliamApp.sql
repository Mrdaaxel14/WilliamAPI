-- ===========================================
--   CREACI�N DE BASE DE DATOS
-- ===========================================
CREATE DATABASE WilliamStoreDB;
GO

USE WilliamStoreDB;
GO

-- ===========================================
--   TABLA: USUARIOS
--   (Clientes que usan la app)
-- ===========================================
CREATE TABLE Usuarios (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Rol VARCHAR(20) DEFAULT 'Cliente',
    FechaRegistro DATETIME DEFAULT GETDATE()
);

GO


-- ===========================================
--   TABLA: CATEGOR�AS
-- ===========================================
CREATE TABLE Categoria (
    IdCategoria INT IDENTITY(1,1) PRIMARY KEY,
    Descripcion VARCHAR(100) NOT NULL
);
GO


-- ===========================================
--   TABLA: PRODUCTOS
-- ===========================================
CREATE TABLE Producto (
    IdProducto INT IDENTITY(1,1) PRIMARY KEY,
    CodigoBarra VARCHAR(30),
    Descripcion VARCHAR(100) NOT NULL,
    Marca VARCHAR(50),
    IdCategoria INT,
    Precio DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_Producto_Categoria
        FOREIGN KEY(IdCategoria) REFERENCES Categoria(IdCategoria)
);
GO


-- ===========================================
--   TABLA: CARRITO (CABEZA)
--   UN CARRITO POR USUARIO
-- ===========================================
CREATE TABLE Carrito (
    IdCarrito INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,

    CONSTRAINT FK_Carrito_Usuario
        FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO


-- ===========================================
--   TABLA: CARRITO DETALLE
--   Productos agregados al carrito
-- ===========================================
CREATE TABLE CarritoDetalle (
    IdCarritoDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdCarrito INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),

    CONSTRAINT FK_CarritoDetalle_Carrito
        FOREIGN KEY(IdCarrito) REFERENCES Carrito(IdCarrito),

    CONSTRAINT FK_CarritoDetalle_Producto
        FOREIGN KEY(IdProducto) REFERENCES Producto(IdProducto)
);
GO


-- ===========================================
--   TABLA: PEDIDOS (CABECERA)
-- ===========================================
CREATE TABLE Pedido (
    IdPedido INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_Pedido_Usuario
        FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO


-- ===========================================
--   TABLA: PEDIDO DETALLE
-- ===========================================
CREATE TABLE PedidoDetalle (
    IdPedidoDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdPedido INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_PedidoDetalle_Pedido
        FOREIGN KEY(IdPedido) REFERENCES Pedido(IdPedido),

    CONSTRAINT FK_PedidoDetalle_Producto
        FOREIGN KEY(IdProducto) REFERENCES Producto(IdProducto)
);
GO

INSERT INTO Categoria (Descripcion)
VALUES ('Remeras'), ('Pantalones'), ('Camperas');

INSERT INTO Producto (CodigoBarra, Descripcion, Marca, IdCategoria, Precio)
VALUES
('0001', 'Remera Oversize Negra', 'Nike', 1, 18000),
('0002', 'Jean Slim Fit Azul', 'Levis', 2, 35000),
('0003', 'Campera Rompeviento', 'Adidas', 3, 45000);

GO

INSERT INTO Usuarios (Nombre, Email, PasswordHash)
VALUES ('Cliente Test', 'cliente@test.com', '123456'); -- luego cambia a hash real

INSERT INTO Usuarios (Nombre, Email, PasswordHash, Rol)
VALUES ('Admin Test', 'admin@mail.com', '123456', 'Admin'); -- luego cambia a hash real

GO

/*======================================================
=              TABLAS DE ROLES Y PERMISOS              =
======================================================*/

-- ROLES (Admin, Empleado, Cliente)
CREATE TABLE Roles (
    IdRol INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);
GO

-- PERMISOS (Opcional)
CREATE TABLE Permisos (
    IdPermiso INT IDENTITY(1,1) PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL UNIQUE
);
GO

-- RELACIÓN USUARIO ↔ ROL
ALTER TABLE Usuarios
ADD IdRol INT NULL;

ALTER TABLE Usuarios
ADD CONSTRAINT FK_Usuarios_Roles
FOREIGN KEY (IdRol) REFERENCES Roles(IdRol);
GO

/*======================================================
=                TABLA: DIRECCIONES                    =
======================================================*/

CREATE TABLE DireccionesUsuario (
    IdDireccion INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    Provincia VARCHAR(100),
    Ciudad VARCHAR(100),
    Calle VARCHAR(150),
    Numero VARCHAR(20),
    CodigoPostal VARCHAR(20),

    CONSTRAINT FK_Direcciones_Usuario
        FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

/*======================================================
=                TABLA: MÉTODOS DE PAGO                =
======================================================*/

CREATE TABLE MetodoPago (
    IdMetodoPago INT IDENTITY(1,1) PRIMARY KEY,
    Metodo VARCHAR(50) NOT NULL   -- Ej: Efectivo, Tarjeta, MercadoPago
);
GO

ALTER TABLE Pedido
ADD IdMetodoPago INT NULL;

ALTER TABLE Pedido
ADD CONSTRAINT FK_Pedido_MetodoPago
FOREIGN KEY(IdMetodoPago) REFERENCES MetodoPago(IdMetodoPago);
GO

/*======================================================
=             TABLAS DE ESTADOS (Auxiliares)           =
======================================================*/

CREATE TABLE EstadoPedido (
    IdEstadoPedido INT IDENTITY(1,1) PRIMARY KEY,
    Estado VARCHAR(50) UNIQUE NOT NULL
);
GO

CREATE TABLE EstadoPago (
    IdEstadoPago INT IDENTITY(1,1) PRIMARY KEY,
    Estado VARCHAR(50) UNIQUE NOT NULL
);
GO

CREATE TABLE EstadoStock (
    IdEstadoStock INT IDENTITY(1,1) PRIMARY KEY,
    Estado VARCHAR(50) UNIQUE NOT NULL
);
GO

ALTER TABLE Pedido
ADD IdEstadoPedido INT NULL,
    IdEstadoPago INT NULL;

ALTER TABLE Pedido
ADD CONSTRAINT FK_Pedido_EstadoPedido 
    FOREIGN KEY(IdEstadoPedido) REFERENCES EstadoPedido(IdEstadoPedido);

ALTER TABLE Pedido
ADD CONSTRAINT FK_Pedido_EstadoPago
    FOREIGN KEY(IdEstadoPago) REFERENCES EstadoPago(IdEstadoPago);
GO

/*======================================================
=                TABLAS DE INVENTARIO                  =
======================================================*/

-- STOCK POR PRODUCTO
CREATE TABLE Stock (
    IdStock INT IDENTITY(1,1) PRIMARY KEY,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL DEFAULT 0,
    IdEstadoStock INT NULL,
    
    CONSTRAINT FK_Stock_Producto
        FOREIGN KEY(IdProducto) REFERENCES Producto(IdProducto),

    CONSTRAINT FK_Stock_EstadoStock
        FOREIGN KEY(IdEstadoStock) REFERENCES EstadoStock(IdEstadoStock)
);
GO

-- IMÁGENES DE PRODUCTO
CREATE TABLE ImagenProducto (
    IdImagen INT IDENTITY(1,1) PRIMARY KEY,
    IdProducto INT NOT NULL,
    UrlImagen VARCHAR(500) NOT NULL,

    CONSTRAINT FK_Producto_Imagen
        FOREIGN KEY(IdProducto) REFERENCES Producto(IdProducto)
);
GO

/*======================================================
=                  COMPRAS A PROVEEDORES               =
======================================================*/

CREATE TABLE Compra (
    IdCompra INT IDENTITY(1,1) PRIMARY KEY,
    Fecha DATETIME DEFAULT GETDATE(),
    Total DECIMAL(10,2) NOT NULL,
    IdUsuario INT NULL  -- empleado/admin que registró

    CONSTRAINT FK_Compra_Usuario
        FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

CREATE TABLE CompraDetalle (
    IdCompraDetalle INT IDENTITY(1,1) PRIMARY KEY,
    IdCompra INT NOT NULL,
    IdProducto INT NOT NULL,
    Cantidad INT NOT NULL,
    CostoUnitario DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_CompraDetalle_Compra
        FOREIGN KEY(IdCompra) REFERENCES Compra(IdCompra),

    CONSTRAINT FK_CompraDetalle_Producto
        FOREIGN KEY(IdProducto) REFERENCES Producto(IdProducto)
);
GO

/*======================================================
=                     AUDITORÍA                        =
======================================================*/

CREATE TABLE Auditoria (
    IdAuditoria INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NULL,
    Fecha DATETIME DEFAULT GETDATE(),
    Accion VARCHAR(200),
    TablaAfectada VARCHAR(100),
    ValorAnterior TEXT NULL,
    ValorNuevo TEXT NULL,

    CONSTRAINT FK_Auditoria_Usuario
        FOREIGN KEY(IdUsuario) REFERENCES Usuarios(IdUsuario)
);
GO

/*======================================================
=      INSERTS BÁSICOS PARA ROLES Y ESTADOS            =
======================================================*/

INSERT INTO Roles (Nombre)
VALUES ('Admin'), ('Empleado'), ('Cliente');

INSERT INTO EstadoPedido (Estado)
VALUES ('Pendiente'), ('Confirmado'), ('Preparando'), ('Enviado'), ('Entregado'), ('Cancelado');

INSERT INTO EstadoPago (Estado)
VALUES ('Pendiente'), ('Pagado'), ('Rechazado');

INSERT INTO EstadoStock (Estado)
VALUES ('En stock'), ('Bajo'), ('Sin stock');

INSERT INTO MetodoPago (Metodo)
VALUES ('Efectivo'), ('Tarjeta'), ('MercadoPago');
GO
