-- ===========================================
--   CREACIÓN DE BASE DE DATOS
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
--   TABLA: CATEGORÍAS
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


SELECT * FROM Usuarios