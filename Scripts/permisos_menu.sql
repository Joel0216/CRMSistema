-- ========================================================
-- Script Fase B: Tablas de permisos, menu y submenu
-- Base de datos: CRM_Base
-- Tabla de roles existente: roles (RolId PK)
-- ========================================================

USE CRM_Base;
GO

-- Limpiar primero si ya existian tablas parciales
IF OBJECT_ID('SP_Permisos_Save', 'P') IS NOT NULL DROP PROCEDURE SP_Permisos_Save;
IF OBJECT_ID('SP_Permisos_GetByRol', 'P') IS NOT NULL DROP PROCEDURE SP_Permisos_GetByRol;
IF OBJECT_ID('SP_Menu_GetAll', 'P') IS NOT NULL DROP PROCEDURE SP_Menu_GetAll;
GO

IF OBJECT_ID('crm_permiso', 'U') IS NOT NULL
BEGIN
    IF OBJECT_ID('FK_crm_permiso_roles', 'F') IS NOT NULL
        ALTER TABLE crm_permiso DROP CONSTRAINT FK_crm_permiso_roles;
    DROP TABLE crm_permiso;
END
GO

IF OBJECT_ID('crm_submenu', 'U') IS NOT NULL DROP TABLE crm_submenu;
GO

IF OBJECT_ID('crm_menu', 'U') IS NOT NULL DROP TABLE crm_menu;
GO

-- Crear tabla de menus principales
CREATE TABLE crm_menu (
    id INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    icono VARCHAR(100) NULL,
    orden INT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Crear tabla de submenus / enlaces
CREATE TABLE crm_submenu (
    id INT PRIMARY KEY IDENTITY(1,1),
    menu_id INT NOT NULL REFERENCES crm_menu(id),
    nombre VARCHAR(50) NOT NULL,
    controlador VARCHAR(50) NOT NULL,
    accion VARCHAR(50) NOT NULL DEFAULT 'Index',
    icono VARCHAR(100) NULL,
    orden INT NOT NULL DEFAULT 0,
    activo BIT NOT NULL DEFAULT 1
);
GO

-- Crear tabla de permisos por rol
CREATE TABLE crm_permiso (
    id INT PRIMARY KEY IDENTITY(1,1),
    rol_id INT NOT NULL,
    submenu_id INT NOT NULL REFERENCES crm_submenu(id),
    activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT UQ_permiso_rol_submenu UNIQUE (rol_id, submenu_id),
    CONSTRAINT FK_crm_permiso_roles FOREIGN KEY (rol_id) REFERENCES roles(RolId)
);
GO

PRINT 'Tablas creadas.';
GO

-- ========================================================
-- Stored procedures
-- ========================================================

CREATE PROCEDURE SP_Menu_GetAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.id AS menuId,
        m.nombre AS menuNombre,
        m.icono AS menuIcono,
        m.orden AS menuOrden,
        sm.id AS submenuId,
        sm.nombre AS submenuNombre,
        sm.controlador AS submenuControlador,
        sm.accion AS submenuAccion,
        sm.icono AS submenuIcono,
        sm.orden AS submenuOrden
    FROM crm_menu m
    INNER JOIN crm_submenu sm ON sm.menu_id = m.id AND sm.activo = 1
    WHERE m.activo = 1
    ORDER BY m.orden, sm.orden;
END
GO

CREATE PROCEDURE SP_Permisos_GetByRol
    @rolId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        m.id AS menuId,
        m.nombre AS menuNombre,
        m.icono AS menuIcono,
        m.orden AS menuOrden,
        sm.id AS submenuId,
        sm.nombre AS submenuNombre,
        sm.controlador AS submenuControlador,
        sm.accion AS submenuAccion,
        sm.icono AS submenuIcono,
        sm.orden AS submenuOrden
    FROM crm_permiso p
    INNER JOIN crm_submenu sm ON sm.id = p.submenu_id AND sm.activo = 1
    INNER JOIN crm_menu m ON m.id = sm.menu_id AND m.activo = 1
    WHERE p.rol_id = @rolId
      AND p.activo = 1
    ORDER BY m.orden, sm.orden;
END
GO

CREATE PROCEDURE SP_Permisos_Save
    @rolId INT,
    @submenuId INT,
    @activo BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM crm_permiso WHERE rol_id = @rolId AND submenu_id = @submenuId)
    BEGIN
        UPDATE crm_permiso
        SET activo = @activo
        WHERE rol_id = @rolId AND submenu_id = @submenuId;
    END
    ELSE
    BEGIN
        INSERT INTO crm_permiso (rol_id, submenu_id, activo)
        VALUES (@rolId, @submenuId, @activo);
    END
END
GO

PRINT 'Stored procedures creados.';
GO

-- ========================================================
-- Datos iniciales: menus y submenus
-- ========================================================

SET IDENTITY_INSERT crm_menu ON;

MERGE crm_menu AS target
USING (VALUES
    (1, 'PRINCIPAL', NULL, 1, 1),
    (2, 'VENTAS', NULL, 2, 1),
    (3, 'OPERACIONES', NULL, 3, 1),
    (4, 'INDICADORES', NULL, 4, 1),
    (5, 'ADMINISTRACIÓN', NULL, 5, 1)
) AS source (id, nombre, icono, orden, activo)
ON target.id = source.id
WHEN MATCHED THEN
    UPDATE SET nombre = source.nombre, icono = source.icono, orden = source.orden, activo = source.activo
WHEN NOT MATCHED THEN
    INSERT (id, nombre, icono, orden, activo)
    VALUES (source.id, source.nombre, source.icono, source.orden, source.activo);

SET IDENTITY_INSERT crm_menu OFF;
GO

SET IDENTITY_INSERT crm_submenu ON;

MERGE crm_submenu AS target
USING (VALUES
    (1, 1, 'Dashboard', 'Dashboard', 'Index', NULL, 1, 1),
    (2, 2, 'Prospectos', 'Prospectos', 'Index', NULL, 1, 1),
    (3, 2, 'Cotizador', 'Cotizador', 'Index', NULL, 2, 1),
    (4, 2, 'Cotizaciones por Aprobar', 'ValidacionCotizaciones', 'Index', NULL, 3, 1),
    (5, 2, 'Contratos', 'Contratos', 'Index', NULL, 4, 1),
    (6, 2, 'Contratos por Autorizar', 'ContratosPorAutorizar', 'Index', NULL, 5, 1),
    (7, 2, 'Contratos Autorizados', 'ContratosAutorizados', 'Index', NULL, 6, 1),
    (8, 3, 'Rutas Cotizadas', 'RutasCotizadas', 'Index', NULL, 1, 1),
    (9, 3, 'Manifiestos', 'Manifiestos', 'Index', NULL, 2, 1),
    (10, 4, 'Reportes', 'Indicadores', 'Index', 'fa-chart-line', 1, 1),
    (11, 5, 'Usuarios', 'Usuarios', 'Index', 'fa-users-cog', 1, 1),
    (12, 5, 'Registrar usuario', 'Usuarios', 'Index', 'fa-user-plus', 2, 1)
) AS source (id, menu_id, nombre, controlador, accion, icono, orden, activo)
ON target.id = source.id
WHEN MATCHED THEN
    UPDATE SET menu_id = source.menu_id, nombre = source.nombre, controlador = source.controlador,
               accion = source.accion, icono = source.icono, orden = source.orden, activo = source.activo
WHEN NOT MATCHED THEN
    INSERT (id, menu_id, nombre, controlador, accion, icono, orden, activo)
    VALUES (source.id, source.menu_id, source.nombre, source.controlador, source.accion, source.icono, source.orden, source.activo);

SET IDENTITY_INSERT crm_submenu OFF;
GO

-- Verificar datos cargados
SELECT 'Menus' AS tipo, id, nombre, orden FROM crm_menu ORDER BY orden;
SELECT 'Submenus' AS tipo, id, menu_id, nombre, controlador, accion, orden FROM crm_submenu ORDER BY menu_id, orden;
GO
