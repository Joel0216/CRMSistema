-- ============================================================
-- Limpieza selectiva de datos de prueba del CRM nuevo
-- Conserva: usuarios, roles, catálogos, empresas y prospectos base.
-- Borra: contratos, cotizaciones, tratos, servicios, archivos,
--         contactos y sucursales de prueba.
-- Base de datos: CRM_Base
-- ============================================================
:setvar DatabaseName "CRM_Base"
GO
USE $(DatabaseName);
GO

SET NOCOUNT ON;

-- Deshabilitar temporalmente las restricciones de FK
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL';
GO

-- Borrar datos operativos del CRM nuevo (conserva prospectos y empresas)
DELETE FROM crm_prospecto_archivos;
DELETE FROM crm_prospecto_contactos;
DELETE FROM crm_prospecto_sucursales;
DELETE FROM crm_notificaciones_correo;
DELETE FROM crm_servicios_cotizados;
DELETE FROM crm_contratos_autorizados;
DELETE FROM crm_cotizaciones_validacion;
DELETE FROM crm_cotizaciones_borradores;
DELETE FROM crm_tratos;
GO

-- Resetear identidades de las tablas limpiadas
DBCC CHECKIDENT ('crm_prospecto_archivos', RESEED, 0);
DBCC CHECKIDENT ('crm_prospecto_contactos', RESEED, 0);
DBCC CHECKIDENT ('crm_prospecto_sucursales', RESEED, 0);
DBCC CHECKIDENT ('crm_notificaciones_correo', RESEED, 0);
DBCC CHECKIDENT ('crm_servicios_cotizados', RESEED, 0);
DBCC CHECKIDENT ('crm_contratos_autorizados', RESEED, 0);
DBCC CHECKIDENT ('crm_cotizaciones_validacion', RESEED, 0);
DBCC CHECKIDENT ('crm_cotizaciones_borradores', RESEED, 0);
DBCC CHECKIDENT ('crm_tratos', RESEED, 0);
GO

-- Volver a habilitar restricciones de FK
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL';
GO

PRINT 'Pruebas del CRM nuevo limpiadas. Prospectos y empresas se conservaron.';
GO
