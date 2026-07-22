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
    @Estatus VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.crm_contratos_autorizados
    SET Estatus = @Estatus,
        Fecha_Autorizacion = CASE WHEN @Estatus = 'Activo' THEN GETDATE() ELSE Fecha_Autorizacion END
    WHERE Contrato_ID = @Contrato_ID;
END
GO
