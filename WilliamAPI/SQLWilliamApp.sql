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
    Telefono VARCHAR(20),
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

USE WilliamStoreDB;
GO

-- =====================================================
-- PASO 1: Agregar columnas a ImagenProducto
-- =====================================================

-- Agregar columna Orden (0 = principal)
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('ImagenProducto') 
               AND name = 'Orden')
BEGIN
    ALTER TABLE ImagenProducto
    ADD Orden INT NOT NULL DEFAULT 0;
END
GO

-- Agregar columna EsPrincipal
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('ImagenProducto') 
               AND name = 'EsPrincipal')
BEGIN
    ALTER TABLE ImagenProducto
    ADD EsPrincipal BIT NOT NULL DEFAULT 0;
END
GO

-- =====================================================
-- PASO 2: Migrar datos existentes
-- =====================================================

-- Marcar la primera imagen de cada producto como principal
WITH PrimerasImagenes AS (
    SELECT 
        IdImagen,
        IdProducto,
        ROW_NUMBER() OVER (PARTITION BY IdProducto ORDER BY IdImagen) AS Fila
    FROM ImagenProducto
)
UPDATE ImagenProducto
SET 
    EsPrincipal = CASE WHEN pi.Fila = 1 THEN 1 ELSE 0 END,
    Orden = pi.Fila - 1
FROM ImagenProducto ip
INNER JOIN PrimerasImagenes pi ON ip.IdImagen = pi.IdImagen;
GO

-- =====================================================
-- PASO 3: Insertar im�genes de ejemplo (opcional)
-- =====================================================

-- Para los productos existentes que no tengan im�genes
DECLARE @IdProducto1 INT, @IdProducto2 INT, @IdProducto3 INT;

SELECT @IdProducto1 = IdProducto FROM Producto WHERE CodigoBarra = '0001';
SELECT @IdProducto2 = IdProducto FROM Producto WHERE CodigoBarra = '0002';
SELECT @IdProducto3 = IdProducto FROM Producto WHERE CodigoBarra = '0003';

-- Remera Oversize (si no tiene im�genes)
IF NOT EXISTS (SELECT 1 FROM ImagenProducto WHERE IdProducto = @IdProducto1)
BEGIN
    INSERT INTO ImagenProducto (IdProducto, UrlImagen, EsPrincipal, Orden)
    VALUES 
        (@IdProducto1, 'https://www.urbanstaroma.com/cdn/shop/files/p25-nike-hf9606-010.jpg?v=1742558386', 1, 0),
        (@IdProducto1, 'https://www.jeanjail.com.au/cdn/shop/files/NikeSportswearPremiumEssentialsTeeBlack.jpg?v=1761106853&width=500', 0, 1),
        (@IdProducto1, 'https://www.jeanjail.com.au/cdn/shop/files/NikeSportswearPremiumEssentialsTeeBlack1.jpg?v=1761106853&width=500', 0, 2);
END

-- Jean Slim Fit (si no tiene im�genes)
IF NOT EXISTS (SELECT 1 FROM ImagenProducto WHERE IdProducto = @IdProducto2)
BEGIN
    INSERT INTO ImagenProducto (IdProducto, UrlImagen, EsPrincipal, Orden)
    VALUES 
        (@IdProducto2, 'https://img01.ztat.net/article/spp-media-p1/d7b82267c4774a27a6fe848f4db9db15/26d7222c3264488da1b59f225f883380.jpg?imwidth=1800', 1, 0),
        (@IdProducto2, 'https://img01.ztat.net/article/spp-media-p1/1fcf4d94efa54184a290cf3dc443324c/6b31edafd74a453b934ad9ac4d9e3885.jpg?imwidth=1800', 0, 1),
        (@IdProducto2, 'https://img01.ztat.net/article/spp-media-p1/34c3e29e5b20483e9ac5561cb6206762/d0390b9d093a4b809d6e6a7317873e4f.jpg?imwidth=1800', 0, 2);
END

-- Campera Rompeviento (si no tiene im�genes)
IF NOT EXISTS (SELECT 1 FROM ImagenProducto WHERE IdProducto = @IdProducto3)
BEGIN
    INSERT INTO ImagenProducto (IdProducto, UrlImagen, EsPrincipal, Orden)
    VALUES 
        (@IdProducto3, 'https://di2ponv0v5otw.cloudfront.net/posts/2023/02/15/63ed7628ffb5d0ff8f7e1a8e/m_wp_63ed763632c1dc1bcde74082.webp', 1, 0),
        (@IdProducto3, 'https://di2ponv0v5otw.cloudfront.net/posts/2023/02/15/63ed7628ffb5d0ff8f7e1a8e/m_wp_63ed763e4bf9ff7b551766eb.webp', 0, 1);
END
GO
-- =====================================================
-- PASO 4: Crear �ndice para mejor performance
-- =====================================================

IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_ImagenProducto_IdProducto_Orden')
BEGIN
    CREATE INDEX IX_ImagenProducto_IdProducto_Orden 
    ON ImagenProducto(IdProducto, Orden);
END
GO

-- =====================================================
-- VERIFICACI�N
-- =====================================================

-- Ver productos con sus im�genes
SELECT 
    p.IdProducto,
    p.Descripcion,
    i.IdImagen,
    i.UrlImagen,
    i.EsPrincipal,
    i.Orden
FROM Producto p
LEFT JOIN ImagenProducto i ON p.IdProducto = i.IdProducto
ORDER BY p.IdProducto, i.Orden;
GO

PRINT 'Migraci�n completada exitosamente';
GO

-- =====================================================
-- MIGRACIÓN: Agregar Nombre y Stock a Producto
-- =====================================================

USE WilliamStoreDB;
GO

-- Agregar columna Nombre
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('Producto') 
               AND name = 'Nombre')
BEGIN
    ALTER TABLE Producto
    ADD Nombre VARCHAR(150) NULL;  -- NULL temporalmente para migración
END
GO

-- Migrar datos existentes: copiar Descripcion a Nombre
UPDATE Producto
SET Nombre = Descripcion
WHERE Nombre IS NULL;
GO

-- Hacer Nombre NOT NULL después de migrar datos
ALTER TABLE Producto
ALTER COLUMN Nombre VARCHAR(150) NOT NULL;
GO

-- Agregar columna Stock
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID('Producto') 
               AND name = 'Stock')
BEGIN
    ALTER TABLE Producto
    ADD Stock INT NOT NULL DEFAULT 0;
END
GO

-- Crear índice en Nombre para búsquedas
IF NOT EXISTS (SELECT * FROM sys.indexes 
               WHERE name = 'IX_Producto_Nombre')
BEGIN
    CREATE INDEX IX_Producto_Nombre 
    ON Producto(Nombre);
END
GO

-- Agregar estado "Devuelto" a EstadoPedido si no existe
IF NOT EXISTS (SELECT 1 FROM EstadoPedido WHERE Estado = 'Devuelto')
BEGIN
    INSERT INTO EstadoPedido (Estado) VALUES ('Devuelto');
END
GO

PRINT 'Migración de Nombre y Stock completada exitosamente';
GO