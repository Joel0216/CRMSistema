-- ============================================================
-- IMPORTANTE: Ejecutar este script con la base de datos CRM_Base
-- seleccionada en SSMS (selector de base arriba del editor).
-- ============================================================

SET NOCOUNT ON;

-- ============================================================
-- Stored procedures de usuarios y roles
-- ============================================================
-- Los alias de columna deben coincidir con el mapeo del DAL:
--   id, nombre, apellido, correo, usuario, rol, rolId, activo
-- ============================================================

-- ------------------------------------------------------------
-- 1. Listar usuarios activos
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_GetActivos','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_GetActivos;
GO
CREATE PROCEDURE dbo.SP_Usuarios_GetActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UsuarioId AS id,
           u.Nombre AS nombre,
           u.Apellidos AS apellido,
           u.Email AS correo,
           u.Usuario AS usuario,
           r.Nombre AS rol,
           u.RolId AS rolId,
           u.Activo AS activo
    FROM dbo.Usuarios u
    INNER JOIN dbo.Roles r ON r.RolId = u.RolId
    WHERE u.Activo = 1
    ORDER BY u.Nombre, u.Apellidos;
END
GO

-- ------------------------------------------------------------
-- 2. Listar todos los usuarios
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_GetAll','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_GetAll;
GO
CREATE PROCEDURE dbo.SP_Usuarios_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UsuarioId AS id,
           u.Nombre AS nombre,
           u.Apellidos AS apellido,
           u.Email AS correo,
           u.Usuario AS usuario,
           r.Nombre AS rol,
           u.RolId AS rolId,
           u.Activo AS activo
    FROM dbo.Usuarios u
    INNER JOIN dbo.Roles r ON r.RolId = u.RolId
    ORDER BY u.Activo DESC, u.Nombre, u.Apellidos;
END
GO

-- ------------------------------------------------------------
-- 3. Obtener usuario por ID
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_GetById','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_GetById;
GO
CREATE PROCEDURE dbo.SP_Usuarios_GetById
    @id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UsuarioId AS id,
           u.Nombre AS nombre,
           u.Apellidos AS apellido,
           u.Email AS correo,
           u.Usuario AS usuario,
           r.Nombre AS rol,
           u.RolId AS rolId,
           u.Activo AS activo
    FROM dbo.Usuarios u
    INNER JOIN dbo.Roles r ON r.RolId = u.RolId
    WHERE u.UsuarioId = @id;
END
GO

-- ------------------------------------------------------------
-- 4. Validar login
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_ValidarLogin','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_ValidarLogin;
GO
CREATE PROCEDURE dbo.SP_Usuarios_ValidarLogin
    @usuario VARCHAR(50),
    @password VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UsuarioId AS id,
           u.Nombre AS nombre,
           u.Apellidos AS apellido,
           u.Email AS correo,
           u.Usuario AS usuario,
           r.Nombre AS rol,
           u.RolId AS rolId,
           u.Activo AS activo
    FROM dbo.Usuarios u
    INNER JOIN dbo.Roles r ON r.RolId = u.RolId
    WHERE (u.Usuario = @usuario OR u.Email = @usuario)
      AND u.PasswordHash = HASHBYTES('SHA2_256', @password)
      AND u.Activo = 1;
END
GO

-- ------------------------------------------------------------
-- 5. Validar password actual
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_ValidarPassword','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_ValidarPassword;
GO
CREATE PROCEDURE dbo.SP_Usuarios_ValidarPassword
    @usuarioId INT,
    @password VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) AS total
    FROM dbo.Usuarios
    WHERE UsuarioId = @usuarioId
      AND PasswordHash = HASHBYTES('SHA2_256', @password)
      AND Activo = 1;
END
GO

-- ------------------------------------------------------------
-- 6. Verificar si existe usuario (excluyendo opcionalmente uno)
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_Existe','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_Existe;
GO
CREATE PROCEDURE dbo.SP_Usuarios_Existe
    @usuario VARCHAR(50),
    @excluirId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT(1) AS total
    FROM dbo.Usuarios
    WHERE Usuario = @usuario
      AND (@excluirId IS NULL OR UsuarioId <> @excluirId);
END
GO

-- ------------------------------------------------------------
-- 7. Insertar usuario
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_Insert','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_Insert;
GO
CREATE PROCEDURE dbo.SP_Usuarios_Insert
    @nombre VARCHAR(100),
    @apellidos VARCHAR(100),
    @correo VARCHAR(150),
    @usuario VARCHAR(50),
    @password VARCHAR(100),
    @rolId INT,
    @registradoPor INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Usuarios (Nombre, Apellidos, Email, Usuario, PasswordHash, RolId, Activo, FechaCreacion, RegistradoPor)
    VALUES (@nombre, @apellidos, @correo, @usuario, HASHBYTES('SHA2_256', @password), @rolId, 1, GETDATE(), @registradoPor);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS nuevoId;
END
GO

-- ------------------------------------------------------------
-- 8. Actualizar usuario
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_Update','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_Update;
GO
CREATE PROCEDURE dbo.SP_Usuarios_Update
    @id INT,
    @nombre VARCHAR(100),
    @apellidos VARCHAR(100),
    @correo VARCHAR(150),
    @usuario VARCHAR(50),
    @rolId INT,
    @actualizadoPor INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios
    SET Nombre = @nombre,
        Apellidos = @apellidos,
        Email = @correo,
        Usuario = @usuario,
        RolId = @rolId,
        FechaModificacion = GETDATE(),
        ActualizadoPor = @actualizadoPor
    WHERE UsuarioId = @id;
END
GO

-- ------------------------------------------------------------
-- 9. Cambiar contraseña
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_UpdatePassword','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_UpdatePassword;
GO
CREATE PROCEDURE dbo.SP_Usuarios_UpdatePassword
    @usuarioId INT,
    @password VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios
    SET PasswordHash = HASHBYTES('SHA2_256', @password),
        FechaModificacion = GETDATE()
    WHERE UsuarioId = @usuarioId;
END
GO

-- ------------------------------------------------------------
-- 10. Cambiar rol
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_UpdateRol','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_UpdateRol;
GO
CREATE PROCEDURE dbo.SP_Usuarios_UpdateRol
    @usuarioId INT,
    @rolId INT,
    @actualizadoPor INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios
    SET RolId = @rolId,
        FechaModificacion = GETDATE(),
        ActualizadoPor = @actualizadoPor
    WHERE UsuarioId = @usuarioId;
END
GO

-- ------------------------------------------------------------
-- 11. Desactivar usuario
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_Disable','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_Disable;
GO
CREATE PROCEDURE dbo.SP_Usuarios_Disable
    @usuarioId INT,
    @desactivadoPor INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios
    SET Activo = 0,
        FechaModificacion = GETDATE(),
        ActualizadoPor = @desactivadoPor
    WHERE UsuarioId = @usuarioId;
END
GO

-- ------------------------------------------------------------
-- 12. Activar usuario
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Usuarios_Enable','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Usuarios_Enable;
GO
CREATE PROCEDURE dbo.SP_Usuarios_Enable
    @usuarioId INT,
    @activadoPor INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Usuarios
    SET Activo = 1,
        FechaModificacion = GETDATE(),
        ActualizadoPor = @activadoPor
    WHERE UsuarioId = @usuarioId;
END
GO

-- ------------------------------------------------------------
-- 13. Listar roles activos
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Roles_GetActivos','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Roles_GetActivos;
GO
CREATE PROCEDURE dbo.SP_Roles_GetActivos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RolId AS id,
           Nombre AS nombre,
           Descripcion AS descripcion,
           Activo AS activo
    FROM dbo.Roles
    WHERE Activo = 1
    ORDER BY RolId;
END
GO

PRINT 'Stored procedures de usuarios y roles creados/actualizados correctamente.';
GO
