-- ============================================================
-- Agregar domicilios fiscal y de recolección al prospecto
-- para edición de contratos rechazados/por autorizar.
-- Base de datos: CRM_Base
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.crm_prospectos')
                 AND name = 'Domicilio_Fiscal')
BEGIN
    ALTER TABLE dbo.crm_prospectos ADD Domicilio_Fiscal NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID('dbo.crm_prospectos')
                 AND name = 'Domicilio_Recoleccion')
BEGIN
    ALTER TABLE dbo.crm_prospectos ADD Domicilio_Recoleccion NVARCHAR(MAX) NULL;
END
GO

-- Actualizar SP de detalle de contrato para devolver los nuevos campos
IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_ContratosAutorizados_GetById;
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
           p.Domicilio_Fiscal,
           p.Domicilio_Recoleccion,
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
