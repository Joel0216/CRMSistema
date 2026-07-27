GO

SET NOCOUNT ON;

DECLARE @SuperadminRolId INT;
SELECT @SuperadminRolId = RolId FROM dbo.Roles WHERE Nombre = 'Superadmin';

IF @SuperadminRolId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Usuario = 'superadmin')
    BEGIN
        INSERT INTO dbo.Usuarios (Nombre, Apellidos, Email, Usuario, PasswordHash, RolId, Activo, FechaCreacion)
        VALUES (
            'Super',
            'Administrador',
            'admin@cmsana.com.mx',
            'superadmin',
            HASHBYTES('SHA2_256', 'SanaAdmin2026!'),
            @SuperadminRolId,
            1,
            GETDATE()
        );

        PRINT 'Usuario superadmin creado correctamente.';
    END
    ELSE
    BEGIN
        PRINT 'El usuario superadmin ya existe. No se creó uno nuevo.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: No se encontró el rol Superadmin. Ejecuta usuarios_roles.sql primero.';
END
GO
