-- ============================================================
-- Limpieza TOTAL de la base de datos CRM_Base
-- Borra TODO el CRM operativo: empresas, prospectos, tratos,
-- cotizaciones, contratos, servicios cotizados, usuarios y roles.
-- Conserva los catálogos de referencia (servicios de residuos,
-- unidades RME, fases de trato) para que RSU/RME sigan disponibles.
-- Saltea tablas que no existan sin detenerse.
-- ============================================================
:setvar DatabaseName "CRM_Base"
GO
USE $(DatabaseName);
GO

SET NOCOUNT ON;

-- Deshabilitar temporalmente las restricciones de FK
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Helper para borrar contenido de tabla si existe
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

-- Resetear identidades solo si la tabla existe
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
    ('Usuarios')
) AS T(t);
EXEC sp_executesql @reseedSql;
GO

-- Volver a habilitar restricciones de FK
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
GO

PRINT 'Base de datos completamente limpiada. Todos los registros fueron eliminados.';
GO
