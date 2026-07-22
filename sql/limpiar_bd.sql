:setvar DatabaseName "CRM_Base"
-- ============================================================
-- Script de limpieza de datos operativos del CRM
-- Conserva: Roles, Usuarios, catálogos (servicios_residuos,
-- crm_configurador_unidades, crm_fases_trato)
-- Resetea identidades a 1.
-- ============================================================
USE $(DatabaseName);
GO

SET NOCOUNT ON;

-- Deshabilitar temporalmente las restricciones de FK
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Borrar en orden respetando las dependencias principales
DELETE FROM CotizacionDetalle;
DELETE FROM CuentasPorCobrar;
DELETE FROM Oportunidades;
DELETE FROM Ventas;
DELETE FROM Cotizaciones;
DELETE FROM Clientes;
DELETE FROM EtapasVenta;
DELETE FROM Sucursales;

-- Limpiar tablas del CRM nuevo
DELETE FROM crm_prospecto_archivos;
DELETE FROM crm_prospecto_contactos;
DELETE FROM crm_prospecto_sucursales;
DELETE FROM crm_notificaciones_correo;
DELETE FROM crm_servicios_cotizados;
DELETE FROM crm_contratos_autorizados;
DELETE FROM crm_cotizaciones_validacion;
DELETE FROM crm_cotizaciones_borradores;
DELETE FROM crm_tratos;
DELETE FROM crm_prospectos;
DELETE FROM empresas;
GO

-- Resetear identidades (RESEED 0 hace que el siguiente sea 1)
DBCC CHECKIDENT ('CotizacionDetalle', RESEED, 0);
DBCC CHECKIDENT ('CuentasPorCobrar', RESEED, 0);
DBCC CHECKIDENT ('Oportunidades', RESEED, 0);
DBCC CHECKIDENT ('Ventas', RESEED, 0);
DBCC CHECKIDENT ('Cotizaciones', RESEED, 0);
DBCC CHECKIDENT ('Clientes', RESEED, 0);
DBCC CHECKIDENT ('EtapasVenta', RESEED, 0);
DBCC CHECKIDENT ('Sucursales', RESEED, 0);
DBCC CHECKIDENT ('crm_prospecto_archivos', RESEED, 0);
DBCC CHECKIDENT ('crm_prospecto_contactos', RESEED, 0);
DBCC CHECKIDENT ('crm_prospecto_sucursales', RESEED, 0);
DBCC CHECKIDENT ('crm_notificaciones_correo', RESEED, 0);
DBCC CHECKIDENT ('crm_servicios_cotizados', RESEED, 0);
DBCC CHECKIDENT ('crm_contratos_autorizados', RESEED, 0);
DBCC CHECKIDENT ('crm_cotizaciones_validacion', RESEED, 0);
DBCC CHECKIDENT ('crm_cotizaciones_borradores', RESEED, 0);
DBCC CHECKIDENT ('crm_tratos', RESEED, 0);
DBCC CHECKIDENT ('crm_prospectos', RESEED, 0);
DBCC CHECKIDENT ('empresas', RESEED, 0);
GO

-- Volver a habilitar restricciones de FK
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
GO

PRINT 'Limpieza completada. Los próximos registros comenzarán desde 1.';
GO
