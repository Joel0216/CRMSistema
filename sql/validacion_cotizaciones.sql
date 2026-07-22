-- ============================================================
-- Scripts para módulo de Validación de Cotizaciones
-- Base de datos: CRM_Base
-- ============================================================

-- 1. Tabla de solicitudes de validación
IF OBJECT_ID('dbo.crm_cotizaciones_validacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.crm_cotizaciones_validacion (
        Validacion_ID INT IDENTITY(1,1) PRIMARY KEY,
        Prospecto_ID INT NOT NULL,
        Borrador_ID INT NOT NULL,
        Datos_Cotizacion NVARCHAR(MAX) NOT NULL,
        Estatus VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
        Motivo_Rechazo NVARCHAR(MAX) NULL,
        Fecha_Creacion DATETIME NOT NULL DEFAULT GETDATE(),
        Fecha_Actualizacion DATETIME NULL,
        Usuario_Creacion VARCHAR(150) NULL,
        Usuario_Valida VARCHAR(150) NULL
    );
END
GO

-- 2. Tabla de contratos autorizados
IF OBJECT_ID('dbo.crm_contratos_autorizados', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.crm_contratos_autorizados (
        Contrato_ID INT IDENTITY(1,1) PRIMARY KEY,
        Prospecto_ID INT NOT NULL,
        Validacion_ID INT NOT NULL,
        Folio VARCHAR(50) NOT NULL,
        Monto_Mensual DECIMAL(18,2) NULL,
        Estatus VARCHAR(50) NOT NULL DEFAULT 'Activo',
        Fecha_Autorizacion DATETIME NOT NULL DEFAULT GETDATE(),
        Autorizado_Por VARCHAR(150) NULL
    );
END
GO

-- 3. Stored Procedures

IF OBJECT_ID('dbo.SP_CotizacionesValidacion_Insert', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_CotizacionesValidacion_Insert;
GO
CREATE PROCEDURE dbo.SP_CotizacionesValidacion_Insert
    @Prospecto_ID INT,
    @Borrador_ID INT,
    @Datos_Cotizacion NVARCHAR(MAX),
    @Usuario_Creacion VARCHAR(150) = NULL
AS
BEGIN
    INSERT INTO dbo.crm_cotizaciones_validacion
        (Prospecto_ID, Borrador_ID, Datos_Cotizacion, Estatus, Fecha_Creacion, Usuario_Creacion)
    VALUES
        (@Prospecto_ID, @Borrador_ID, @Datos_Cotizacion, 'Pendiente', GETDATE(), @Usuario_Creacion);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Validacion_ID;
END
GO

IF OBJECT_ID('dbo.SP_CotizacionesValidacion_GetPending', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_CotizacionesValidacion_GetPending;
GO
CREATE PROCEDURE dbo.SP_CotizacionesValidacion_GetPending
AS
BEGIN
    SELECT v.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Calle,
           p.Num_Ext,
           p.Colonia,
           p.Municipio,
           ISNULL(u.Nombre + ' ' + u.Apellidos, '') AS VendedorNombre
    FROM dbo.crm_cotizaciones_validacion v
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = v.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Vendedor_ID
    WHERE v.Estatus = 'Pendiente'
    ORDER BY v.Fecha_Actualizacion DESC, v.Fecha_Creacion DESC;
END
GO

IF OBJECT_ID('dbo.SP_CotizacionesValidacion_GetById', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_CotizacionesValidacion_GetById;
GO
CREATE PROCEDURE dbo.SP_CotizacionesValidacion_GetById
    @Validacion_ID INT
AS
BEGIN
    SELECT v.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Nombre_Comercial,
           p.Calle,
           p.Num_Ext,
           p.Colonia,
           p.Municipio,
           p.CP,
           p.Estado,
           p.Telefono,
           p.Correo,
           p.Tipo_Persona,
           ISNULL(u.Nombre + ' ' + u.Apellidos, '') AS VendedorNombre
    FROM dbo.crm_cotizaciones_validacion v
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = v.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Vendedor_ID
    WHERE v.Validacion_ID = @Validacion_ID;
END
GO

IF OBJECT_ID('dbo.SP_CotizacionesValidacion_GetByProspecto', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_CotizacionesValidacion_GetByProspecto;
GO
CREATE PROCEDURE dbo.SP_CotizacionesValidacion_GetByProspecto
    @Prospecto_ID INT
AS
BEGIN
    SELECT TOP 1 *
    FROM dbo.crm_cotizaciones_validacion
    WHERE Prospecto_ID = @Prospecto_ID
    ORDER BY Fecha_Creacion DESC;
END
GO

IF OBJECT_ID('dbo.SP_CotizacionesValidacion_UpdateEstatus', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_CotizacionesValidacion_UpdateEstatus;
GO
CREATE PROCEDURE dbo.SP_CotizacionesValidacion_UpdateEstatus
    @Validacion_ID INT,
    @Estatus VARCHAR(50),
    @Motivo_Rechazo NVARCHAR(MAX) = NULL,
    @Usuario_Valida VARCHAR(150) = NULL
AS
BEGIN
    UPDATE dbo.crm_cotizaciones_validacion
    SET Estatus = @Estatus,
        Motivo_Rechazo = @Motivo_Rechazo,
        Fecha_Actualizacion = GETDATE(),
        Usuario_Valida = @Usuario_Valida
    WHERE Validacion_ID = @Validacion_ID;
END
GO

IF OBJECT_ID('dbo.SP_ContratosAutorizados_Insert', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_Insert;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_Insert
    @Prospecto_ID INT,
    @Validacion_ID INT,
    @Folio VARCHAR(50),
    @Monto_Mensual DECIMAL(18,2) = NULL,
    @Autorizado_Por VARCHAR(150) = NULL
AS
BEGIN
    INSERT INTO dbo.crm_contratos_autorizados
        (Prospecto_ID, Validacion_ID, Folio, Monto_Mensual, Estatus, Fecha_Autorizacion, Autorizado_Por)
    VALUES
        (@Prospecto_ID, @Validacion_ID, @Folio, @Monto_Mensual, 'Activo', GETDATE(), @Autorizado_Por);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Contrato_ID;
END
GO

IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetAll', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_GetAll;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_GetAll
AS
BEGIN
    SELECT c.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Calle,
           p.Num_Ext,
           p.Colonia,
           p.Municipio
    FROM dbo.crm_contratos_autorizados c
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = c.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    ORDER BY c.Fecha_Autorizacion DESC;
END
GO

IF OBJECT_ID('dbo.SP_Prospectos_UpdateEstatus', 'P') IS NOT NULL
BEGIN
    -- Asegurar que el SP existente soporte los nuevos estatus.
    -- Si ya existe, no se recrea para no romper otras dependencias.
    PRINT 'SP_Prospectos_UpdateEstatus ya existe.';
END
GO

-- ============================================================
-- SP para insertar servicios cotizados (alineado con C#)
-- ============================================================
IF OBJECT_ID('dbo.SP_ServiciosCotizados_Insert', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ServiciosCotizados_Insert;
GO
CREATE PROCEDURE dbo.SP_ServiciosCotizados_Insert
    @Trato_ID INT,
    @Tipo_Residuo NVARCHAR(250),
    @Frecuencia NVARCHAR(50),
    @Periodicidad_Pago NVARCHAR(50),
    @Volumen_Estimado DECIMAL(18,2) = NULL,
    @Precio_Unitario DECIMAL(18,2) = NULL,
    @Dias_Asignados NVARCHAR(100),
    @Porcentaje_Adicional DECIMAL(5,2) = NULL,
    @Porcentaje_Descuento DECIMAL(5,2) = NULL,
    @Sucursal_ID NVARCHAR(50) = NULL,
    @Tipo_Unidad NVARCHAR(100) = NULL,
    @Tipo_Cobro NVARCHAR(100) = NULL,
    @Recolectores INT = NULL,
    @Turno NVARCHAR(50) = NULL,
    @Ruta NVARCHAR(250) = NULL,
    @Costo_Tonelada DECIMAL(18,2) = NULL,
    @Costo_Disposicion DECIMAL(18,2) = NULL
AS
BEGIN
    INSERT INTO dbo.crm_servicios_cotizados
        (Trato_ID, Tipo_Residuo, Frecuencia, Periodicidad_Pago, Volumen_Estimado,
         Precio_Unitario, Dias_Asignados, Porcentaje_Adicional, Porcentaje_Descuento,
         Sucursal_ID, Tipo_Unidad, Tipo_Cobro, Recolectores, Turno, Ruta,
         Costo_Tonelada, Costo_Disposicion, Fecha_Creacion)
    VALUES
        (@Trato_ID, @Tipo_Residuo, @Frecuencia, @Periodicidad_Pago, @Volumen_Estimado,
         @Precio_Unitario, @Dias_Asignados, @Porcentaje_Adicional, @Porcentaje_Descuento,
         @Sucursal_ID, @Tipo_Unidad, @Tipo_Cobro, @Recolectores, @Turno, @Ruta,
         @Costo_Tonelada, @Costo_Disposicion, GETDATE());

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS ID;
END
GO

-- ============================================================
-- SP para obtener servicios cotizados de un trato (completo)
-- ============================================================
IF OBJECT_ID('dbo.SP_ServiciosCotizados_GetByTrato', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ServiciosCotizados_GetByTrato;
GO
CREATE PROCEDURE dbo.SP_ServiciosCotizados_GetByTrato
    @Trato_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT id, trato_id, tipo_residuo, frecuencia, periodicidad_pago, volumen_estimado, precio_unitario,
           dias_asignados, porcentaje_adicional, porcentaje_descuento, sucursal_id, Tipo_Unidad,
           Tipo_Cobro, Recolectores, Turno, Ruta, Limpieza_Extra, Costo_Renta, Combustible,
           Recorrido_Servicio, Costo_Tonelada, Costo_Disposicion, Capacidad_Toneladas, fecha_creacion
    FROM dbo.crm_servicios_cotizados
    WHERE trato_id = @Trato_ID;
END
GO
