-- ========================================================
-- Script Fase B: Asignar permisos por rol
-- Base de datos: CRM_Base
-- Requiere haber ejecutado primero roles_jefe_coordinador.sql y permisos_menu.sql
-- ========================================================

USE CRM_Base;
GO

-- Limpiar permisos existentes para evitar duplicados
DELETE FROM crm_permiso;
GO

DECLARE @rolVendedor INT, @rolSupervisor INT, @rolCoordinador INT, @rolJefe INT, @rolSuperadmin INT;

SELECT @rolVendedor = RolId FROM roles WHERE LOWER(Nombre) = 'vendedor';
SELECT @rolSupervisor = RolId FROM roles WHERE LOWER(Nombre) = 'supervisor';
SELECT @rolCoordinador = RolId FROM roles WHERE LOWER(Nombre) = 'coordinador';
SELECT @rolJefe = RolId FROM roles WHERE LOWER(Nombre) = 'jefe';
SELECT @rolSuperadmin = RolId FROM roles WHERE LOWER(Nombre) = 'superadmin';

IF @rolVendedor IS NULL OR @rolSupervisor IS NULL OR @rolCoordinador IS NULL OR @rolJefe IS NULL OR @rolSuperadmin IS NULL
BEGIN
    RAISERROR('Faltan roles base. Verifica que existan Vendedor, Supervisor, Coordinador, Jefe y Superadmin en la tabla roles.', 16, 1);
    RETURN;
END

PRINT 'Roles encontrados.';
PRINT 'Vendedor: ' + CAST(@rolVendedor AS VARCHAR);
PRINT 'Supervisor: ' + CAST(@rolSupervisor AS VARCHAR);
PRINT 'Coordinador: ' + CAST(@rolCoordinador AS VARCHAR);
PRINT 'Jefe: ' + CAST(@rolJefe AS VARCHAR);
PRINT 'Superadmin: ' + CAST(@rolSuperadmin AS VARCHAR);
GO

-- ========================================================
-- Asignar permisos
-- ========================================================

-- Vendedor: Dashboard, Prospectos, Cotizador, Contratos, Contratos Autorizados
INSERT INTO crm_permiso (rol_id, submenu_id, activo)
SELECT r.RolId, sm.id, 1
FROM roles r
CROSS JOIN crm_submenu sm
WHERE LOWER(r.Nombre) = 'vendedor'
  AND sm.nombre IN ('Dashboard', 'Prospectos', 'Cotizador', 'Contratos', 'Contratos Autorizados');

-- Supervisor: todo lo de Vendedor + Cotizaciones por Aprobar + Contratos por Autorizar
INSERT INTO crm_permiso (rol_id, submenu_id, activo)
SELECT r.RolId, sm.id, 1
FROM roles r
CROSS JOIN crm_submenu sm
WHERE LOWER(r.Nombre) = 'supervisor'
  AND sm.nombre IN (
      'Dashboard', 'Prospectos', 'Cotizador', 'Contratos', 'Contratos Autorizados',
      'Cotizaciones por Aprobar', 'Contratos por Autorizar'
  );

-- Coordinador: todo lo de Supervisor + Rutas Cotizadas + Manifiestos
INSERT INTO crm_permiso (rol_id, submenu_id, activo)
SELECT r.RolId, sm.id, 1
FROM roles r
CROSS JOIN crm_submenu sm
WHERE LOWER(r.Nombre) = 'coordinador'
  AND sm.nombre IN (
      'Dashboard', 'Prospectos', 'Cotizador', 'Contratos', 'Contratos Autorizados',
      'Cotizaciones por Aprobar', 'Contratos por Autorizar',
      'Rutas Cotizadas', 'Manifiestos'
  );

-- Jefe: todo lo de Coordinador + Usuarios + Registrar usuario
INSERT INTO crm_permiso (rol_id, submenu_id, activo)
SELECT r.RolId, sm.id, 1
FROM roles r
CROSS JOIN crm_submenu sm
WHERE LOWER(r.Nombre) = 'jefe'
  AND sm.nombre IN (
      'Dashboard', 'Prospectos', 'Cotizador', 'Contratos', 'Contratos Autorizados',
      'Cotizaciones por Aprobar', 'Contratos por Autorizar',
      'Rutas Cotizadas', 'Manifiestos',
      'Usuarios', 'Registrar usuario'
  );

-- Superadmin: todos los submenus activos
INSERT INTO crm_permiso (rol_id, submenu_id, activo)
SELECT r.RolId, sm.id, 1
FROM roles r
CROSS JOIN crm_submenu sm
WHERE LOWER(r.Nombre) = 'superadmin'
  AND sm.activo = 1;

-- Administrador (alias de Supervisor): mismos permisos que Supervisor
INSERT INTO crm_permiso (rol_id, submenu_id, activo)
SELECT r.RolId, sm.id, 1
FROM roles r
CROSS JOIN crm_submenu sm
WHERE LOWER(r.Nombre) = 'administrador'
  AND sm.nombre IN (
      'Dashboard', 'Prospectos', 'Cotizador', 'Contratos', 'Contratos Autorizados',
      'Cotizaciones por Aprobar', 'Contratos por Autorizar'
  );
GO

-- Verificar permisos asignados
SELECT r.Nombre AS rol, m.nombre AS menu, sm.nombre AS submenu
FROM crm_permiso p
INNER JOIN roles r ON r.RolId = p.rol_id
INNER JOIN crm_submenu sm ON sm.id = p.submenu_id
INNER JOIN crm_menu m ON m.id = sm.menu_id
WHERE p.activo = 1
ORDER BY r.Nombre, m.orden, sm.orden;
GO
