-- ============================================================
-- IMPORTANTE: Ejecutar este script con la base de datos CRM_Base
-- seleccionada en SSMS.
-- ============================================================

SET NOCOUNT ON;

-- ============================================================
-- Stored procedures del dashboard
-- ============================================================
-- Estos SPs alimentan al DashboardController. Los alias de columna
-- deben coincidir exactamente con las propiedades que lee el C#:
--   mes, total, ingresos, tratos, nombre, cantidad, tipo,
--   estatus, empresa, contacto, tipoInmueble, fuente, trato,
--   monto, fase, tieneSucursales, vendedorNombre, fecha,
--   tipo_residuo, frecuencia, periodicidad_pago, volumen_estimado,
--   precio_unitario, ingMes, ingAnt, prosMes, prosAnt, cotServ,
--   cotBorr, deudores, alCorriente, prosSuc, totalSuc, totalP, convP.
-- ============================================================

-- ------------------------------------------------------------
-- 1. KPIs del mes actual y anterior
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetKPIs','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetKPIs;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetKPIs
    @inicioMes DATETIME,
    @finMes DATETIME,
    @inicioMesAnt DATETIME,
    @finMesAnt DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ingMes DECIMAL(18,2) = 0, @ingAnt DECIMAL(18,2) = 0;
    DECLARE @prosMes INT = 0, @prosAnt INT = 0;
    DECLARE @cotServ INT = 0, @cotBorr INT = 0;
    DECLARE @deudores INT = 0, @alCorriente INT = 0;
    DECLARE @prosSuc INT = 0, @totalSuc INT = 0;
    DECLARE @totalP INT = 0, @convP INT = 0;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_tratos') AND name = 'Fecha_Creacion')
    BEGIN
        SELECT @ingMes = ISNULL(SUM(Importe),0) FROM dbo.crm_tratos
        WHERE Fecha_Creacion BETWEEN @inicioMes AND @finMes;

        SELECT @ingAnt = ISNULL(SUM(Importe),0) FROM dbo.crm_tratos
        WHERE Fecha_Creacion BETWEEN @inicioMesAnt AND @finMesAnt;
    END
    ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_tratos') AND name = 'Fecha_Cierre_Estimada')
    BEGIN
        SELECT @ingMes = ISNULL(SUM(Importe),0) FROM dbo.crm_tratos
        WHERE Fecha_Cierre_Estimada BETWEEN @inicioMes AND @finMes;

        SELECT @ingAnt = ISNULL(SUM(Importe),0) FROM dbo.crm_tratos
        WHERE Fecha_Cierre_Estimada BETWEEN @inicioMesAnt AND @finMesAnt;
    END

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_prospectos') AND name = 'Fecha_Creacion')
    BEGIN
        SELECT @prosMes = COUNT(*) FROM dbo.crm_prospectos
        WHERE Fecha_Creacion BETWEEN @inicioMes AND @finMes;

        SELECT @prosAnt = COUNT(*) FROM dbo.crm_prospectos
        WHERE Fecha_Creacion BETWEEN @inicioMesAnt AND @finMesAnt;
    END

    IF OBJECT_ID('dbo.crm_tratos') IS NOT NULL
        SELECT @cotServ = COUNT(*) FROM dbo.crm_tratos
        WHERE Fase_ID IN (1,2,3);

    IF OBJECT_ID('dbo.crm_cotizaciones_borradores') IS NOT NULL
        SELECT @cotBorr = COUNT(*) FROM dbo.crm_cotizaciones_borradores;

    SELECT @deudores = COUNT(*) FROM dbo.crm_prospectos
    WHERE LOWER(ISNULL(Estatus,'')) LIKE '%adeudo%';

    SELECT @alCorriente = COUNT(*) FROM dbo.crm_prospectos
    WHERE LOWER(ISNULL(Estatus,'')) IN ('aprobado','cotizado','en seguimiento');

    IF OBJECT_ID('dbo.crm_prospecto_sucursales') IS NOT NULL
    BEGIN
        SELECT @prosSuc = COUNT(DISTINCT Prospecto_ID) FROM dbo.crm_prospecto_sucursales;
        SELECT @totalSuc = COUNT(*) FROM dbo.crm_prospecto_sucursales;
    END

    SELECT @totalP = COUNT(*) FROM dbo.crm_prospectos;

    IF OBJECT_ID('dbo.crm_tratos') IS NOT NULL
        SELECT @convP = COUNT(DISTINCT Prospecto_ID) FROM dbo.crm_tratos;

    SELECT
        @ingMes AS ingMes,
        @ingAnt AS ingAnt,
        @prosMes AS prosMes,
        @prosAnt AS prosAnt,
        @cotServ AS cotServ,
        @cotBorr AS cotBorr,
        @deudores AS deudores,
        @alCorriente AS alCorriente,
        @prosSuc AS prosSuc,
        @totalSuc AS totalSuc,
        @totalP AS totalP,
        @convP AS convP;
END
GO

-- ------------------------------------------------------------
-- 2. Tendencia de prospectos (últimos 6 meses)
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetTendenciaProspectos','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetTendenciaProspectos;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetTendenciaProspectos
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_prospectos') AND name = 'Fecha_Creacion')
    BEGIN
        SELECT FORMAT(Fecha_Creacion,'yyyy-MM') AS mes, COUNT(*) AS total
        FROM dbo.crm_prospectos
        WHERE Fecha_Creacion >= DATEADD(MONTH,-6,CAST(GETDATE() AS DATE))
        GROUP BY FORMAT(Fecha_Creacion,'yyyy-MM')
        ORDER BY mes;
    END
    ELSE
    BEGIN
        SELECT '' AS mes, 0 AS total WHERE 1 = 0;
    END
END
GO

-- ------------------------------------------------------------
-- 3. Tendencia de ventas/tratos
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetTendenciaVentas','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetTendenciaVentas;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetTendenciaVentas
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_tratos') AND name = 'Fecha_Creacion')
    BEGIN
        SELECT FORMAT(Fecha_Creacion,'yyyy-MM') AS mes,
               ISNULL(SUM(Importe),0) AS ingresos,
               COUNT(*) AS tratos
        FROM dbo.crm_tratos
        WHERE Fecha_Creacion >= DATEADD(MONTH,-6,CAST(GETDATE() AS DATE))
        GROUP BY FORMAT(Fecha_Creacion,'yyyy-MM')
        ORDER BY mes;
    END
    ELSE IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_tratos') AND name = 'Fecha_Cierre_Estimada')
    BEGIN
        SELECT FORMAT(Fecha_Cierre_Estimada,'yyyy-MM') AS mes,
               ISNULL(SUM(Importe),0) AS ingresos,
               COUNT(*) AS tratos
        FROM dbo.crm_tratos
        WHERE Fecha_Cierre_Estimada >= DATEADD(MONTH,-6,CAST(GETDATE() AS DATE))
        GROUP BY FORMAT(Fecha_Cierre_Estimada,'yyyy-MM')
        ORDER BY mes;
    END
    ELSE
    BEGIN
        SELECT '' AS mes, 0.0 AS ingresos, 0 AS tratos WHERE 1 = 0;
    END
END
GO

-- ------------------------------------------------------------
-- 4. Orígenes de prospectos
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetOrigenes','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetOrigenes;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetOrigenes
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_prospectos') AND name = 'Fuente_ID')
    BEGIN
        SELECT ISNULL(CAST(p.Fuente_ID AS VARCHAR(50)),'Sin origen') AS nombre,
               COUNT(*) AS cantidad
        FROM dbo.crm_prospectos p
        WHERE p.Fuente_ID IS NOT NULL
        GROUP BY p.Fuente_ID
        ORDER BY cantidad DESC;
    END
    ELSE
    BEGIN
        SELECT 'Desconocido' AS nombre, 0 AS cantidad WHERE 1 = 0;
    END
END
GO

-- ------------------------------------------------------------
-- 5. Tipos de inmueble
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetTiposInmueble','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetTiposInmueble;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetTiposInmueble
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_prospectos') AND name = 'Tipo_Inmueble')
    BEGIN
        SELECT ISNULL(Tipo_Inmueble,'Sin tipo') AS tipo, COUNT(*) AS cantidad
        FROM dbo.crm_prospectos
        GROUP BY Tipo_Inmueble
        ORDER BY cantidad DESC;
    END
    ELSE
    BEGIN
        SELECT 'Sin tipo' AS tipo, 0 AS cantidad WHERE 1 = 0;
    END
END
GO

-- ------------------------------------------------------------
-- 6. Distribución de estatus del mes
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetEstatusDistribucionPorMes','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetEstatusDistribucionPorMes;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetEstatusDistribucionPorMes
    @inicioMes DATETIME,
    @finMes DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_prospectos') AND name = 'Fecha_Creacion')
    BEGIN
        SELECT ISNULL(Estatus,'Sin estatus') AS estatus, COUNT(*) AS cantidad
        FROM dbo.crm_prospectos
        WHERE Fecha_Creacion BETWEEN @inicioMes AND @finMes
        GROUP BY Estatus
        ORDER BY cantidad DESC;
    END
    ELSE
    BEGIN
        SELECT 'Sin estatus' AS estatus, 0 AS cantidad WHERE 1 = 0;
    END
END
GO

-- ------------------------------------------------------------
-- 7. Pipeline inmediato
--    IMPORTANTE: el alias vendedorNombre debe ir en minúscula exacta.
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetPipeline','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetPipeline;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetPipeline
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX);

    SET @sql = N'
    SELECT
        p.Prospecto_ID AS id,
        ISNULL(p.Nombre_Comercial_Empresa, p.Nombre_Prospecto) AS empresa,
        p.Nombre_Prospecto AS contacto,
        ISNULL(p.Estatus,''Nuevo'') AS estatus,
        ISNULL(p.Tipo_Inmueble,'''') AS tipoInmueble,
        ISNULL(CAST(p.Fuente_ID AS VARCHAR(50)),'''') AS fuente,
        ISNULL(t.Nombre_Trato,'''') AS trato,
        ISNULL(t.Importe,0) AS monto,';

    IF OBJECT_ID('dbo.crm_fases_venta') IS NOT NULL
        SET @sql = @sql + N'
        ISNULL(f.Nombre_Fase,'''') AS fase,';
    ELSE
        SET @sql = @sql + N'
        '''' AS fase,';

    SET @sql = @sql + N'
        CASE WHEN LOWER(ISNULL(p.Tiene_Sucursales,''No'')) LIKE ''s%'' THEN ''Sí'' ELSE ''No'' END AS tieneSucursales,
        ISNULL(u.Nombre,'''') + '' '' + ISNULL(u.Apellidos,'''') AS vendedorNombre,
        p.Fecha_Creacion AS fecha
    FROM dbo.crm_prospectos p
    LEFT JOIN (
        SELECT Prospecto_ID, MAX(Trato_ID) AS ultimo
        FROM dbo.crm_tratos
        GROUP BY Prospecto_ID
    ) tmax ON tmax.Prospecto_ID = p.Prospecto_ID
    LEFT JOIN dbo.crm_tratos t ON t.Trato_ID = tmax.ultimo
    LEFT JOIN dbo.Usuarios u ON u.UsuarioId = p.Propietario_ID';

    IF OBJECT_ID('dbo.crm_fases_venta') IS NOT NULL
        SET @sql = @sql + N'
    LEFT JOIN dbo.crm_fases_venta f ON f.Fase_ID = t.Fase_ID';

    SET @sql = @sql + N'
    ORDER BY p.Fecha_Creacion DESC;';

    EXEC sp_executesql @sql;
END
GO

-- ------------------------------------------------------------
-- 8. Detalle de cotizaciones recientes
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Dashboard_GetCotizacionesDetalle','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Dashboard_GetCotizacionesDetalle;
GO
CREATE PROCEDURE dbo.SP_Dashboard_GetCotizacionesDetalle
AS
BEGIN
    SET NOCOUNT ON;

    IF OBJECT_ID('dbo.crm_servicios_cotizados') IS NULL
    BEGIN
        SELECT '' AS tipo_residuo, '' AS frecuencia, '' AS periodicidad_pago,
               0 AS volumen_estimado, 0 AS precio_unitario, '' AS trato, '' AS empresa
        WHERE 1 = 0;
        RETURN;
    END

    DECLARE @sql2 NVARCHAR(MAX);
    SET @sql2 = N'
        SELECT
            sc.Tipo_Residuo AS tipo_residuo,
            sc.Frecuencia AS frecuencia,
            sc.Periodicidad_Pago AS periodicidad_pago,
            ISNULL(sc.Volumen_Estimado,0) AS volumen_estimado,
            ISNULL(sc.Precio_Unitario,0) AS precio_unitario,
            ISNULL(t.Nombre_Trato,'''') AS trato,';

    IF OBJECT_ID('dbo.empresas') IS NOT NULL
        SET @sql2 = @sql2 + N'
            ISNULL(e.Nombre_Empresa, ISNULL(p.Nombre_Comercial_Empresa, p.Nombre_Prospecto)) AS empresa';
    ELSE
        SET @sql2 = @sql2 + N'
            ISNULL(p.Nombre_Comercial_Empresa, p.Nombre_Prospecto) AS empresa';

    SET @sql2 = @sql2 + N'
        FROM dbo.crm_servicios_cotizados sc
        INNER JOIN dbo.crm_tratos t ON t.Trato_ID = sc.Trato_ID
        INNER JOIN dbo.crm_prospectos p ON p.Prospecto_ID = t.Prospecto_ID';

    IF OBJECT_ID('dbo.empresas') IS NOT NULL
        SET @sql2 = @sql2 + N'
        LEFT JOIN dbo.empresas e ON e.Empresa_ID = p.Empresa_ID';

    IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.crm_servicios_cotizados') AND name = 'fecha_creacion')
        SET @sql2 = @sql2 + N'
        ORDER BY sc.fecha_creacion DESC;';
    ELSE
        SET @sql2 = @sql2 + N';';

    EXEC sp_executesql @sql2;
END
GO

PRINT 'Stored procedures del dashboard creados/actualizados correctamente.';
GO
