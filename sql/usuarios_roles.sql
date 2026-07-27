-- ============================================================
-- Creación/Actualización de tablas Roles y Usuarios para el CRM
-- ============================================================
:setvar DatabaseName "CRM_Base"
GO
USE $(DatabaseName);
GO

SET NOCOUNT ON;

-- ------------------------------------------------------------
-- 1. Tabla Roles
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RolId INT PRIMARY KEY IDENTITY(1,1),
        Nombre VARCHAR(50) NOT NULL UNIQUE,
        Descripcion VARCHAR(255) NULL,
        Activo BIT DEFAULT 1
    );

    INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES
    ('Vendedor', 'Crea prospectos y cotizaciones. Solo ve sus registros.'),
    ('Supervisor', 'Aprueba cotizaciones/contratos y asigna rutas operativas.'),
    ('Superadmin', 'Acceso total: usuarios, catálogos, configuración y todos los módulos.');
END
ELSE
BEGIN
    -- Asegurar que los roles base existan
    IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Vendedor')
        INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES ('Vendedor', 'Crea prospectos y cotizaciones. Solo ve sus registros.');
    IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Supervisor')
        INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES ('Supervisor', 'Aprueba cotizaciones/contratos y asigna rutas operativas.');
    IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Superadmin')
        INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES ('Superadmin', 'Acceso total: usuarios, catálogos, configuración y todos los módulos.');
END
GO

-- ------------------------------------------------------------
-- 2. Tabla Usuarios
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioId INT PRIMARY KEY IDENTITY(1,1),
        Nombre VARCHAR(100) NOT NULL,
        Apellidos VARCHAR(100) NULL,
        Email VARCHAR(150) NULL,
        Usuario VARCHAR(50) NOT NULL UNIQUE,
        PasswordHash VARBINARY(64) NOT NULL,
        RolId INT NOT NULL FOREIGN KEY REFERENCES dbo.Roles(RolId),
        Activo BIT DEFAULT 1,
        FechaCreacion DATETIME DEFAULT GETDATE(),
        FechaModificacion DATETIME NULL,
        RegistradoPor INT NULL,
        ActualizadoPor INT NULL
    );
END
ELSE
BEGIN
    -- Asegurar columnas que usa la aplicación (DAL) si la tabla ya existía con estructura anterior
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'RegistradoPor' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD RegistradoPor INT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ActualizadoPor' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD ActualizadoPor INT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FechaCreacion' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD FechaCreacion DATETIME NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FechaModificacion' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD FechaModificacion DATETIME NULL;
    -- Mantener compatibilidad con nombres anteriores
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FechaRegistro' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD FechaRegistro DATETIME NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FechaActualizacion' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD FechaActualizacion DATETIME NULL;
END
GO

-- ------------------------------------------------------------
-- 3. Índices útiles
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuarios_RolId' AND object_id = OBJECT_ID('dbo.Usuarios'))
    CREATE NONCLUSTERED INDEX IX_Usuarios_RolId ON dbo.Usuarios(RolId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuarios_Activo' AND object_id = OBJECT_ID('dbo.Usuarios'))
    CREATE NONCLUSTERED INDEX IX_Usuarios_Activo ON dbo.Usuarios(Activo);
GO

PRINT 'Tablas Roles y Usuarios verificadas/creadas correctamente.';
GO
