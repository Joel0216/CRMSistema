-- ============================================================
-- IMPORTANTE: Ejecutar este script con la base de datos CRM_Base
-- seleccionada en SSMS.
-- ============================================================

SET NOCOUNT ON;

-- ============================================================
-- Stored procedures del módulo cotizador
-- ============================================================
-- SPs faltantes que reemplazan SQL embebido en CotizacionesDAL.cs
-- ============================================================

-- ------------------------------------------------------------
-- 1. Actualizar datos JSON de un borrador
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Cotizaciones_UpdateBorrador','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Cotizaciones_UpdateBorrador;
GO
CREATE PROCEDURE dbo.SP_Cotizaciones_UpdateBorrador
    @Borrador_ID INT,
    @Datos_Borrador NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.crm_cotizaciones_borradores
    SET Datos_Borrador = @Datos_Borrador
    WHERE Borrador_ID = @Borrador_ID;
END
GO

-- ------------------------------------------------------------
-- 2. Listar validaciones de un prospecto
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_CotizacionesValidacion_GetByProspecto','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_CotizacionesValidacion_GetByProspecto;
GO
CREATE PROCEDURE dbo.SP_CotizacionesValidacion_GetByProspecto
    @Prospecto_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Validacion_ID,
           Prospecto_ID,
           Borrador_ID,
           Datos_Cotizacion,
           Estatus,
           Motivo_Rechazo,
           Fecha_Creacion,
           Fecha_Actualizacion,
           Usuario_Creacion,
           Usuario_Valida
    FROM dbo.crm_cotizaciones_validacion
    WHERE Prospecto_ID = @Prospecto_ID
    ORDER BY Validacion_ID DESC;
END
GO

-- ------------------------------------------------------------
-- 3. Obtener validación por borrador (preferir autorizada, luego la más reciente)
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_CotizacionesValidacion_GetByBorrador','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_CotizacionesValidacion_GetByBorrador;
GO
CREATE PROCEDURE dbo.SP_CotizacionesValidacion_GetByBorrador
    @Borrador_ID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Preferir la validación autorizada más reciente
    IF EXISTS (SELECT 1 FROM dbo.crm_cotizaciones_validacion WHERE Borrador_ID = @Borrador_ID AND Estatus = 'Autorizada')
    BEGIN
        SELECT TOP 1
               Validacion_ID,
               Prospecto_ID,
               Borrador_ID,
               Datos_Cotizacion,
               Estatus,
               Motivo_Rechazo,
               Fecha_Creacion,
               Fecha_Actualizacion,
               Usuario_Creacion,
               Usuario_Valida
        FROM dbo.crm_cotizaciones_validacion
        WHERE Borrador_ID = @Borrador_ID
          AND Estatus = 'Autorizada'
        ORDER BY Validacion_ID DESC;
        RETURN;
    END

    -- Si no hay autorizadas, devolver la más reciente
    SELECT TOP 1
           Validacion_ID,
           Prospecto_ID,
           Borrador_ID,
           Datos_Cotizacion,
           Estatus,
           Motivo_Rechazo,
           Fecha_Creacion,
           Fecha_Actualizacion,
           Usuario_Creacion,
           Usuario_Valida
    FROM dbo.crm_cotizaciones_validacion
    WHERE Borrador_ID = @Borrador_ID
    ORDER BY Validacion_ID DESC;
END
GO

PRINT 'Stored procedures del cotizador creados/actualizados correctamente.';
GO
