-- ============================================================
-- IMPORTANTE: Ejecutar este script con la base de datos CRM_Base
-- seleccionada en SSMS.
-- ============================================================

SET NOCOUNT ON;

-- ============================================================
-- Stored procedures del módulo de contratos
-- ============================================================
-- SPs faltantes que reemplazan SQL embebido en ContratosDAL.cs
-- ============================================================

-- ------------------------------------------------------------
-- 1. Obtener contrato autorizado por Validacion_ID
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_ContratosAutorizados_GetByValidacion','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_ContratosAutorizados_GetByValidacion;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_GetByValidacion
    @Validacion_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP 1
           Contrato_ID,
           Prospecto_ID,
           Validacion_ID,
           Folio,
           Monto_Mensual,
           Estatus,
           Motivo_Rechazo,
           Usuario_Rechaza,
           Fecha_Rechazo,
           Fecha_Autorizacion,
           Autorizado_Por
    FROM dbo.crm_contratos_autorizados
    WHERE Validacion_ID = @Validacion_ID
    ORDER BY Contrato_ID DESC;
END
GO

-- ------------------------------------------------------------
-- 2. Actualizar monto mensual de un contrato autorizado
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_ContratosAutorizados_UpdateMontoMensual','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_ContratosAutorizados_UpdateMontoMensual;
GO
CREATE PROCEDURE dbo.SP_ContratosAutorizados_UpdateMontoMensual
    @Contrato_ID INT,
    @Monto_Mensual DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.crm_contratos_autorizados
    SET Monto_Mensual = @Monto_Mensual
    WHERE Contrato_ID = @Contrato_ID;
END
GO

PRINT 'Stored procedures de contratos creados/actualizados correctamente.';
GO
