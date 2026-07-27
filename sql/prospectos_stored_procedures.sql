-- ============================================================
-- IMPORTANTE: Ejecutar este script con la base de datos CRM_Base
-- seleccionada en SSMS.
-- ============================================================

SET NOCOUNT ON;

-- ============================================================
-- Stored procedures del módulo prospectos
-- ============================================================
-- SPs faltantes que reemplazan SQL embebido en ApiProspectosDAL.cs
-- ============================================================

-- ------------------------------------------------------------
-- 1. Recuperar ID del prospecto más reciente por RFC/Correo
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Prospectos_GetIdByRfcCorreo','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Prospectos_GetIdByRfcCorreo;
GO
CREATE PROCEDURE dbo.SP_Prospectos_GetIdByRfcCorreo
    @RFC VARCHAR(20) = NULL,
    @Correo VARCHAR(150) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- El esquema actual no incluye RFC; se usa Correo + Nombre_Comercial_Empresa como fallback.
    SELECT TOP 1 Prospecto_ID AS id
    FROM dbo.crm_prospectos
    WHERE (@Correo IS NULL OR Correo = @Correo)
    ORDER BY Prospecto_ID DESC;
END
GO

-- ------------------------------------------------------------
-- 2. Actualizar datos básicos desde edición de contratos
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Prospectos_UpdateBasicoDesdeContrato','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Prospectos_UpdateBasicoDesdeContrato;
GO
CREATE PROCEDURE dbo.SP_Prospectos_UpdateBasicoDesdeContrato
    @Prospecto_ID INT,
    @Nombre_Prospecto VARCHAR(200),
    @Nombre_Comercial_Empresa VARCHAR(200),
    @Nombre_Comercial VARCHAR(200),
    @RFC VARCHAR(20),
    @Correo VARCHAR(150),
    @Telefono VARCHAR(50),
    @Folio_Catastral VARCHAR(100),
    @Domicilio_Fiscal NVARCHAR(MAX),
    @Domicilio_Recoleccion NVARCHAR(MAX),
    @ActualizarDireccion BIT,
    @Calle VARCHAR(200),
    @Num_Ext VARCHAR(50),
    @Num_Int VARCHAR(50),
    @Colonia VARCHAR(100),
    @Municipio VARCHAR(100),
    @CP VARCHAR(10),
    @Estado VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX);

    SET @sql = N'
    UPDATE dbo.crm_prospectos
    SET Nombre_Prospecto = @Nombre_Prospecto,
        Nombre_Comercial_Empresa = @Nombre_Comercial_Empresa,
        Nombre_Comercial = @Nombre_Comercial,
        Correo = @Correo,
        Telefono = @Telefono,
        Folio_Catastral = @Folio_Catastral,
        Domicilio_Fiscal = @Domicilio_Fiscal,
        Domicilio_Recoleccion = @Domicilio_Recoleccion';

    IF @ActualizarDireccion = 1
    BEGIN
        IF COL_LENGTH('dbo.crm_prospectos','Calle') IS NOT NULL
            SET @sql = @sql + N', Calle = @Calle';
        IF COL_LENGTH('dbo.crm_prospectos','Num_Ext') IS NOT NULL
            SET @sql = @sql + N', Num_Ext = @Num_Ext';
        IF COL_LENGTH('dbo.crm_prospectos','Num_Int') IS NOT NULL
            SET @sql = @sql + N', Num_Int = @Num_Int';
        IF COL_LENGTH('dbo.crm_prospectos','Colonia') IS NOT NULL
            SET @sql = @sql + N', Colonia = @Colonia';
        IF COL_LENGTH('dbo.crm_prospectos','Municipio') IS NOT NULL
            SET @sql = @sql + N', Municipio = @Municipio';
        IF COL_LENGTH('dbo.crm_prospectos','CP') IS NOT NULL
            SET @sql = @sql + N', CP = @CP';
        IF COL_LENGTH('dbo.crm_prospectos','Estado') IS NOT NULL
            SET @sql = @sql + N', Estado = @Estado';
    END

    SET @sql = @sql + N' WHERE Prospecto_ID = @Prospecto_ID;';

    EXEC sp_executesql @sql,
        N'@Prospecto_ID INT, @Nombre_Prospecto VARCHAR(200), @Nombre_Comercial_Empresa VARCHAR(200), @Nombre_Comercial VARCHAR(200),
          @Correo VARCHAR(150), @Telefono VARCHAR(50), @Folio_Catastral VARCHAR(100), @Domicilio_Fiscal NVARCHAR(MAX),
          @Domicilio_Recoleccion NVARCHAR(MAX), @Calle VARCHAR(200), @Num_Ext VARCHAR(50), @Num_Int VARCHAR(50),
          @Colonia VARCHAR(100), @Municipio VARCHAR(100), @CP VARCHAR(10), @Estado VARCHAR(100)',
        @Prospecto_ID, @Nombre_Prospecto, @Nombre_Comercial_Empresa, @Nombre_Comercial,
        @Correo, @Telefono, @Folio_Catastral, @Domicilio_Fiscal, @Domicilio_Recoleccion,
        @Calle, @Num_Ext, @Num_Int, @Colonia, @Municipio, @CP, @Estado;
END
GO

-- ------------------------------------------------------------
-- 3. Upsert del contacto representante legal de un prospecto
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_ProspectoContactos_UpsertRepresentanteLegal','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_ProspectoContactos_UpsertRepresentanteLegal;
GO
CREATE PROCEDURE dbo.SP_ProspectoContactos_UpsertRepresentanteLegal
    @Prospecto_ID INT,
    @Nombre_Contacto VARCHAR(200),
    @Correo VARCHAR(150),
    @Telefono VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.crm_prospecto_contactos WHERE Prospecto_ID = @Prospecto_ID AND Representante_Legal = 1)
    BEGIN
        UPDATE dbo.crm_prospecto_contactos
        SET Nombre_Contacto = @Nombre_Contacto
        WHERE Prospecto_ID = @Prospecto_ID AND Representante_Legal = 1;
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.crm_prospecto_contactos WHERE Prospecto_ID = @Prospecto_ID)
    BEGIN
        UPDATE TOP (1) dbo.crm_prospecto_contactos
        SET Nombre_Contacto = @Nombre_Contacto,
            Representante_Legal = 1
        WHERE Prospecto_ID = @Prospecto_ID;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.crm_prospecto_contactos (Prospecto_ID, Nombre_Contacto, Representante_Legal, Correo, Telefono)
        VALUES (@Prospecto_ID, @Nombre_Contacto, 1, @Correo, @Telefono);
    END
END
GO

-- ------------------------------------------------------------
-- 4. Actualizar domicilios, folio catastral y archivos de un prospecto
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Prospectos_UpdateArchivosYFolio','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Prospectos_UpdateArchivosYFolio;
GO
CREATE PROCEDURE dbo.SP_Prospectos_UpdateArchivosYFolio
    @Prospecto_ID INT,
    @Domicilio_Fiscal NVARCHAR(MAX),
    @Domicilio_Recoleccion NVARCHAR(MAX),
    @Folio_Catastral VARCHAR(100),
    @Foto_Fachada VARBINARY(MAX),
    @Foto_Acceso VARBINARY(MAX),
    @Foto_Referencia VARBINARY(MAX),
    @Documento_Catastral VARBINARY(MAX),
    @Documento_Catastral_Nombre VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @sql NVARCHAR(MAX);

    SET @sql = N'UPDATE dbo.crm_prospectos SET ';

    IF COL_LENGTH('dbo.crm_prospectos','Domicilio_Fiscal') IS NOT NULL
        SET @sql = @sql + N'Domicilio_Fiscal = ISNULL(NULLIF(@Domicilio_Fiscal, ''''), Domicilio_Fiscal), ';
    IF COL_LENGTH('dbo.crm_prospectos','Domicilio_Recoleccion') IS NOT NULL
        SET @sql = @sql + N'Domicilio_Recoleccion = ISNULL(NULLIF(@Domicilio_Recoleccion, ''''), Domicilio_Recoleccion), ';
    IF COL_LENGTH('dbo.crm_prospectos','Folio_Catastral') IS NOT NULL
        SET @sql = @sql + N'Folio_Catastral = ISNULL(NULLIF(@Folio_Catastral, ''''), Folio_Catastral), ';
    IF COL_LENGTH('dbo.crm_prospectos','Foto_Fachada') IS NOT NULL
        SET @sql = @sql + N'Foto_Fachada = ISNULL(@Foto_Fachada, Foto_Fachada), ';
    IF COL_LENGTH('dbo.crm_prospectos','Foto_Acceso') IS NOT NULL
        SET @sql = @sql + N'Foto_Acceso = ISNULL(@Foto_Acceso, Foto_Acceso), ';
    IF COL_LENGTH('dbo.crm_prospectos','Foto_Referencia') IS NOT NULL
        SET @sql = @sql + N'Foto_Referencia = ISNULL(@Foto_Referencia, Foto_Referencia), ';
    IF COL_LENGTH('dbo.crm_prospectos','Documento_Catastral') IS NOT NULL
        SET @sql = @sql + N'Documento_Catastral = ISNULL(@Documento_Catastral, Documento_Catastral), ';
    IF COL_LENGTH('dbo.crm_prospectos','Documento_Catastral_Nombre') IS NOT NULL
        SET @sql = @sql + N'Documento_Catastral_Nombre = ISNULL(@Documento_Catastral_Nombre, Documento_Catastral_Nombre), ';

    -- Quitar última coma y agregar WHERE
    SET @sql = LEFT(@sql, LEN(@sql) - 1) + N' WHERE Prospecto_ID = @Prospecto_ID;';

    EXEC sp_executesql @sql,
        N'@Prospecto_ID INT, @Domicilio_Fiscal NVARCHAR(MAX), @Domicilio_Recoleccion NVARCHAR(MAX),
          @Folio_Catastral VARCHAR(100), @Foto_Fachada VARBINARY(MAX), @Foto_Acceso VARBINARY(MAX),
          @Foto_Referencia VARBINARY(MAX), @Documento_Catastral VARBINARY(MAX), @Documento_Catastral_Nombre VARCHAR(255)',
        @Prospecto_ID, @Domicilio_Fiscal, @Domicilio_Recoleccion, @Folio_Catastral,
        @Foto_Fachada, @Foto_Acceso, @Foto_Referencia, @Documento_Catastral, @Documento_Catastral_Nombre;
END
GO

-- ------------------------------------------------------------
-- 5. Listar notificaciones de correo de un prospecto
-- ------------------------------------------------------------
IF OBJECT_ID('dbo.SP_Notificaciones_GetByProspecto','P') IS NOT NULL
    DROP PROCEDURE dbo.SP_Notificaciones_GetByProspecto;
GO
CREATE PROCEDURE dbo.SP_Notificaciones_GetByProspecto
    @Prospecto_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    -- El esquema actual de crm_notificaciones_correo solo tiene estas columnas.
    SELECT Notificacion_ID,
           Prospecto_ID,
           Tipo_Asunto,
           Correo_Destino,
           Password_Temporal,
           Cotizacion_Ref,
           Fecha_Envio
    FROM dbo.crm_notificaciones_correo
    WHERE Prospecto_ID = @Prospecto_ID
    ORDER BY Fecha_Envio DESC;
END
GO

PRINT 'Stored procedures de prospectos creados/actualizados correctamente.';
GO
