:setvar DatabaseName "CRM_Base"
GO
USE $(DatabaseName);
GO

SET NOCOUNT ON;

-- ============================================================
-- 1. Limpieza TOTAL
-- ============================================================
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql += 'IF OBJECT_ID(''' + t + ''', ''U'') IS NOT NULL DELETE FROM ' + QUOTENAME(t) + '; '
FROM (VALUES
    ('CotizacionDetalle'),
    ('CuentasPorCobrar'),
    ('Oportunidades'),
    ('Ventas'),
    ('Cotizaciones'),
    ('Clientes'),
    ('EtapasVenta'),
    ('Sucursales'),
    ('crm_prospecto_archivos'),
    ('crm_prospecto_contactos'),
    ('crm_prospecto_sucursales'),
    ('crm_notificaciones_correo'),
    ('crm_servicios_cotizados'),
    ('crm_contratos_autorizados'),
    ('crm_cotizaciones_validacion'),
    ('crm_cotizaciones_borradores'),
    ('crm_tratos'),
    ('crm_prospectos'),
    ('empresas'),
    ('UsuarioRoles'),
    ('Usuarios'),
    ('Roles')
) AS T(t);
EXEC sp_executesql @sql;
GO

DECLARE @reseedSql NVARCHAR(MAX) = '';
SELECT @reseedSql += 'IF OBJECT_ID(''' + t + ''', ''U'') IS NOT NULL DBCC CHECKIDENT (''' + QUOTENAME(t) + ''', RESEED, 0); '
FROM (VALUES
    ('CotizacionDetalle'),
    ('CuentasPorCobrar'),
    ('Oportunidades'),
    ('Ventas'),
    ('Cotizaciones'),
    ('Clientes'),
    ('EtapasVenta'),
    ('Sucursales'),
    ('crm_prospecto_archivos'),
    ('crm_prospecto_contactos'),
    ('crm_prospecto_sucursales'),
    ('crm_notificaciones_correo'),
    ('crm_servicios_cotizados'),
    ('crm_contratos_autorizados'),
    ('crm_cotizaciones_validacion'),
    ('crm_cotizaciones_borradores'),
    ('crm_tratos'),
    ('crm_prospectos'),
    ('empresas'),
    ('Usuarios'),
    ('Roles')
) AS T(t);
EXEC sp_executesql @reseedSql;
GO

EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
GO

PRINT 'Base de datos limpiada completamente.';
GO

-- ============================================================
-- 2. Crear/Verificar tabla Usuarios con columnas correctas
-- ============================================================
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RolId INT PRIMARY KEY IDENTITY(1,1),
        Nombre VARCHAR(50) NOT NULL UNIQUE,
        Descripcion VARCHAR(255) NULL,
        Activo BIT DEFAULT 1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Vendedor')
    INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES ('Vendedor', 'Crea prospectos y cotizaciones. Solo ve sus registros.');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Supervisor')
    INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES ('Supervisor', 'Aprueba cotizaciones/contratos y asigna rutas operativas.');
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Nombre = 'Superadmin')
    INSERT INTO dbo.Roles (Nombre, Descripcion) VALUES ('Superadmin', 'Acceso total: usuarios, catálogos, configuración y todos los módulos.');
GO

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
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'RegistradoPor' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD RegistradoPor INT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ActualizadoPor' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD ActualizadoPor INT NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FechaCreacion' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD FechaCreacion DATETIME NULL;
    IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'FechaModificacion' AND object_id = OBJECT_ID('dbo.Usuarios'))
        ALTER TABLE dbo.Usuarios ADD FechaModificacion DATETIME NULL;
END
GO

-- ============================================================
-- 3. Crear usuario Superadmin
-- ============================================================
DECLARE @SuperadminRolId INT;
SELECT @SuperadminRolId = RolId FROM dbo.Roles WHERE Nombre = 'Superadmin';

IF @SuperadminRolId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Usuario = 'superadmin')
BEGIN
    INSERT INTO dbo.Usuarios (Nombre, Apellidos, Email, Usuario, PasswordHash, RolId, Activo, FechaCreacion)
    VALUES (
        'Super',
        'Administrador',
        'admin@cmsana.com.mx',
        'superadmin',
        HASHBYTES('SHA2_256', 'SanaAdmin2026!'),
        @SuperadminRolId,
        1,
        GETDATE()
    );

    PRINT 'Usuario superadmin creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El superadmin ya existe o no se encontro el rol Superadmin.';
END
GO

PRINT 'Proceso finalizado. Inicia sesion con superadmin / SanaAdmin2026!';
GO
