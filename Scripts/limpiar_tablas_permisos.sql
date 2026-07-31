-- Limpia tablas de permisos si quedaron creadas parcialmente
USE CRM_Base;
GO

IF OBJECT_ID('SP_Permisos_Save', 'P') IS NOT NULL DROP PROCEDURE SP_Permisos_Save;
IF OBJECT_ID('SP_Permisos_GetByRol', 'P') IS NOT NULL DROP PROCEDURE SP_Permisos_GetByRol;
IF OBJECT_ID('SP_Menu_GetAll', 'P') IS NOT NULL DROP PROCEDURE SP_Menu_GetAll;
GO

IF OBJECT_ID('crm_permiso', 'U') IS NOT NULL
BEGIN
    ALTER TABLE crm_permiso DROP CONSTRAINT FK__crm_permiso__roles;
    DROP TABLE crm_permiso;
END
GO

IF OBJECT_ID('crm_submenu', 'U') IS NOT NULL DROP TABLE crm_submenu;
GO

IF OBJECT_ID('crm_menu', 'U') IS NOT NULL DROP TABLE crm_menu;
GO

PRINT 'Tablas de permisos limpiadas.';
GO
