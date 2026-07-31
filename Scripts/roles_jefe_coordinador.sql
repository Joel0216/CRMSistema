-- Script para registrar los nuevos roles Jefe y Coordinador en CRM_Base.
-- Tabla de roles: roles (RolId, Nombre, Descripcion, Activo, FechaCreacion)

USE CRM_Base;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'roles')
BEGIN
    RAISERROR('No se encontro la tabla roles.', 16, 1);
    RETURN;
END
GO

IF NOT EXISTS (SELECT 1 FROM roles WHERE LOWER(Nombre) = 'jefe')
BEGIN
    INSERT INTO roles (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Jefe', 'Rol gerencial superior. Supervisa coordinadores y tiene visibilidad global de ventas y operaciones.', 1, GETDATE());
    PRINT 'Rol Jefe registrado.';
END
ELSE
BEGIN
    PRINT 'Rol Jefe ya existe.';
END
GO

IF NOT EXISTS (SELECT 1 FROM roles WHERE LOWER(Nombre) = 'coordinador')
BEGIN
    INSERT INTO roles (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Coordinador', 'Rol de coordinacion. Supervisa a supervisores y vendedores de su area.', 1, GETDATE());
    PRINT 'Rol Coordinador registrado.';
END
ELSE
BEGIN
    PRINT 'Rol Coordinador ya existe.';
END
GO

-- Verificar roles activos
SELECT RolId, Nombre, Descripcion, Activo
FROM roles
WHERE LOWER(Nombre) IN ('vendedor', 'supervisor', 'coordinador', 'jefe', 'superadmin', 'administrador')
ORDER BY RolId;
GO
