-- ============================================================
-- Scripts adicionales para módulo de Contratos
-- Base de datos: CRM_Base
-- ============================================================
-- Estos SPs extienden la funcionalidad de crm_contratos_autorizados
-- creada originalmente en validacion_cotizaciones.sql.
-- No crea tablas nuevas.
-- ============================================================

IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetById', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_GetById;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_GetById
    @Contrato_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Nombre_Comercial,
           p.Nombre_Prospecto AS Contacto,
           p.Telefono,
           p.Correo,
           p.Tipo_Persona,
           p.Calle,
           p.Num_Ext,
           p.Num_Int,
           p.Colonia,
           p.Municipio,
           p.CP,
           p.Estado,
           p.Referencias,
           p.Folio_Catastral,
           p.Dias_Disponibles,
           p.Horario,
           p.Ruta,
           ISNULL(u.Nombre + ' ' + u.Apellidos, '') AS VendedorNombre
    FROM dbo.crm_contratos_autorizados c
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = c.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Vendedor_ID
    WHERE c.Contrato_ID = @Contrato_ID;
END
GO

IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetByEstatus', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_GetByEstatus;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_GetByEstatus
    @Estatus VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Calle,
           p.Num_Ext,
           p.Colonia,
           p.Municipio,
           p.Telefono,
           p.Correo,
           p.Ruta,
           p.Horario,
           ISNULL(u.Nombre + ' ' + u.Apellidos, '') AS VendedorNombre
    FROM dbo.crm_contratos_autorizados c
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = c.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Vendedor_ID
    WHERE (@Estatus IS NULL OR c.Estatus = @Estatus)
    ORDER BY c.Fecha_Autorizacion DESC;
END
GO

IF OBJECT_ID('dbo.SP_ContratosAutorizados_UpdateEstatus', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_UpdateEstatus;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_UpdateEstatus
    @Contrato_ID INT,
    @Estatus VARCHAR(50),
    @Motivo_Rechazo NVARCHAR(MAX) = NULL,
    @Usuario_Rechaza VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.crm_contratos_autorizados
    SET Estatus = @Estatus,
        Fecha_Autorizacion = CASE WHEN @Estatus = 'Activo' THEN GETDATE() ELSE Fecha_Autorizacion END,
        Motivo_Rechazo = CASE WHEN @Estatus = 'Rechazado' THEN @Motivo_Rechazo ELSE Motivo_Rechazo END,
        Usuario_Rechaza = CASE WHEN @Estatus = 'Rechazado' THEN @Usuario_Rechaza ELSE Usuario_Rechaza END,
        Fecha_Rechazo = CASE WHEN @Estatus = 'Rechazado' THEN GETDATE() ELSE Fecha_Rechazo END
    WHERE Contrato_ID = @Contrato_ID;
END
GO

-- ============================================================
-- Extensiones para flujo "Contratos por Autorizar"
-- ============================================================

-- 1. Agregar columnas de rechazo si no existen
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_contratos_autorizados') AND name = 'Motivo_Rechazo')
    ALTER TABLE dbo.crm_contratos_autorizados ADD Motivo_Rechazo NVARCHAR(MAX) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_contratos_autorizados') AND name = 'Usuario_Rechaza')
    ALTER TABLE dbo.crm_contratos_autorizados ADD Usuario_Rechaza VARCHAR(150) NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_contratos_autorizados') AND name = 'Fecha_Rechazo')
    ALTER TABLE dbo.crm_contratos_autorizados ADD Fecha_Rechazo DATETIME NULL;
GO

-- 2. Cambiar default de estatus a 'Por Autorizar'
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
        (@Prospecto_ID, @Validacion_ID, @Folio, @Monto_Mensual, 'Por Autorizar', GETDATE(), @Autorizado_Por);

    SELECT CAST(SCOPE_IDENTITY() AS INT) AS Contrato_ID;
END
GO

-- 3. SPs de consulta incluyendo datos de rechazo
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

IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetByEstatus', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_GetByEstatus;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_GetByEstatus
    @Estatus VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Calle,
           p.Num_Ext,
           p.Colonia,
           p.Municipio,
           p.Telefono,
           p.Correo,
           p.Ruta,
           p.Horario,
           ISNULL(u.Nombre + ' ' + u.Apellidos, '') AS VendedorNombre
    FROM dbo.crm_contratos_autorizados c
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = c.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Vendedor_ID
    WHERE (@Estatus IS NULL OR c.Estatus = @Estatus)
    ORDER BY c.Fecha_Autorizacion DESC;
END
GO

IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetById', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_GetById;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_GetById
    @Contrato_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Nombre_Comercial,
           p.Nombre_Prospecto AS Contacto,
           p.Telefono,
           p.Correo,
           p.Tipo_Persona,
           p.Calle,
           p.Num_Ext,
           p.Num_Int,
           p.Colonia,
           p.Municipio,
           p.CP,
           p.Estado,
           p.Referencias,
           p.Folio_Catastral,
           p.Dias_Disponibles,
           p.Horario,
           p.Ruta,
           ISNULL(u.Nombre + ' ' + u.Apellidos, '') AS VendedorNombre
    FROM dbo.crm_contratos_autorizados c
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = c.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Vendedor_ID
    WHERE c.Contrato_ID = @Contrato_ID;
END
GO

-- 4. Contratos pendientes de autorizar
IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetPending', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ContratosAutorizados_GetPending;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_GetPending
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*,
           e.Nombre_Empresa AS RazonSocial,
           e.RFC,
           p.Calle,
           p.Num_Ext,
           p.Colonia,
           p.Municipio,
           p.Telefono,
           p.Correo,
           p.Ruta,
           p.Horario,
           ISNULL(u.Nombre + ' ' + u.Apellidos, '') AS VendedorNombre
    FROM dbo.crm_contratos_autorizados c
    INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = c.Prospecto_ID
    LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Vendedor_ID
    WHERE c.Estatus = 'Por Autorizar'
    ORDER BY c.Fecha_Autorizacion DESC;
END
GO

-- 5. SP para actualizar servicios cotizados (usado al editar contrato rechazado)
IF OBJECT_ID('dbo.SP_ServiciosCotizados_Update', 'P') IS NOT NULL DROP PROCEDURE dbo.SP_ServiciosCotizados_Update;
GO
CREATE PROCEDURE dbo.SP_ServiciosCotizados_Update
    @ID INT,
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
    SET NOCOUNT ON;
    UPDATE dbo.crm_servicios_cotizados
    SET Tipo_Residuo = @Tipo_Residuo,
        Frecuencia = @Frecuencia,
        Periodicidad_Pago = @Periodicidad_Pago,
        Volumen_Estimado = @Volumen_Estimado,
        Precio_Unitario = @Precio_Unitario,
        Dias_Asignados = @Dias_Asignados,
        Porcentaje_Adicional = @Porcentaje_Adicional,
        Porcentaje_Descuento = @Porcentaje_Descuento,
        Sucursal_ID = @Sucursal_ID,
        Tipo_Unidad = @Tipo_Unidad,
        Tipo_Cobro = @Tipo_Cobro,
        Recolectores = @Recolectores,
        Turno = @Turno,
        Ruta = @Ruta,
        Costo_Tonelada = @Costo_Tonelada,
        Costo_Disposicion = @Costo_Disposicion
    WHERE id = @ID;
END
GO
