-- Script de base de datos para soportar servicios RME/RSU de cobro ÚNICO.
-- Ejecutar en SQL Server contra la base del CRM.

-- 1. Agregar columna Fecha_Unica si no existe.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'Fecha_Unica'
      AND Object_ID = Object_ID(N'ServiciosCotizados')
)
BEGIN
    ALTER TABLE ServiciosCotizados
    ADD Fecha_Unica VARCHAR(20) NULL;
END
GO

-- 2. Actualizar SP de inserción para recibir y guardar Fecha_Unica.
IF OBJECT_ID('SP_ServiciosCotizados_Insert', 'P') IS NOT NULL
    DROP PROCEDURE SP_ServiciosCotizados_Insert;
GO
CREATE PROCEDURE SP_ServiciosCotizados_Insert
    @Trato_ID INT,
    @Tipo_Residuo VARCHAR(50),
    @Frecuencia VARCHAR(50),
    @Periodicidad_Pago VARCHAR(50),
    @Volumen_Estimado DECIMAL(18,2) = NULL,
    @Precio_Unitario DECIMAL(18,2) = NULL,
    @Dias_Asignados VARCHAR(100),
    @Porcentaje_Adicional DECIMAL(18,2) = NULL,
    @Porcentaje_Descuento DECIMAL(18,2) = NULL,
    @Sucursal_ID INT = NULL,
    @Tipo_Unidad VARCHAR(50) = NULL,
    @Tipo_Cobro VARCHAR(50) = NULL,
    @Recolectores INT = NULL,
    @Turno VARCHAR(50) = NULL,
    @Fecha_Unica VARCHAR(20) = NULL,
    @Ruta VARCHAR(100) = NULL,
    @Costo_Tonelada DECIMAL(18,2) = NULL,
    @Costo_Disposicion DECIMAL(18,2) = NULL
AS
BEGIN
    INSERT INTO ServiciosCotizados
    (
        Trato_ID, Tipo_Residuo, Frecuencia, Periodicidad_Pago,
        Volumen_Estimado, Precio_Unitario, Dias_Asignados,
        Porcentaje_Adicional, Porcentaje_Descuento, Sucursal_ID,
        Tipo_Unidad, Tipo_Cobro, Recolectores, Turno, Fecha_Unica,
        Ruta, Costo_Tonelada, Costo_Disposicion
    )
    VALUES
    (
        @Trato_ID, @Tipo_Residuo, @Frecuencia, @Periodicidad_Pago,
        @Volumen_Estimado, @Precio_Unitario, @Dias_Asignados,
        @Porcentaje_Adicional, @Porcentaje_Descuento, @Sucursal_ID,
        @Tipo_Unidad, @Tipo_Cobro, @Recolectores, @Turno, @Fecha_Unica,
        @Ruta, @Costo_Tonelada, @Costo_Disposicion
    );

    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
END
GO

-- 3. Actualizar SP de actualización para recibir y guardar Fecha_Unica.
IF OBJECT_ID('SP_ServiciosCotizados_Update', 'P') IS NOT NULL
    DROP PROCEDURE SP_ServiciosCotizados_Update;
GO
CREATE PROCEDURE SP_ServiciosCotizados_Update
    @ID INT,
    @Tipo_Residuo VARCHAR(50),
    @Frecuencia VARCHAR(50),
    @Periodicidad_Pago VARCHAR(50),
    @Volumen_Estimado DECIMAL(18,2) = NULL,
    @Precio_Unitario DECIMAL(18,2) = NULL,
    @Dias_Asignados VARCHAR(100) = NULL,
    @Porcentaje_Adicional DECIMAL(18,2) = NULL,
    @Porcentaje_Descuento DECIMAL(18,2) = NULL,
    @Sucursal_ID INT = NULL,
    @Tipo_Unidad VARCHAR(50) = NULL,
    @Tipo_Cobro VARCHAR(50) = NULL,
    @Recolectores INT = NULL,
    @Turno VARCHAR(50) = NULL,
    @Fecha_Unica VARCHAR(20) = NULL,
    @Ruta VARCHAR(100) = NULL,
    @Costo_Tonelada DECIMAL(18,2) = NULL,
    @Costo_Disposicion DECIMAL(18,2) = NULL
AS
BEGIN
    UPDATE ServiciosCotizados
    SET Tipo_Residuo = @Tipo_Residuo,
        Frecuencia = @Frecuencia,
        Periodicidad_Pago = @Periodicidad_Pago,
        Volumen_Estimado = @Volumen_Estimado,
        Precio_Unitario = @Precio_Unitario,
        Dias_Asignados = @Dias_Asignados,
        Porcentaje_Adicional = @Porcentaje_Adicional,
        Porcentaje_Descuento = @Porcentaje_Descuento,
        Sucursal_ID = @Sucursal_ID,
        Tipo_Unidad = @Tipo_Unidad,
        Tipo_Cobro = @Tipo_Cobro,
        Recolectores = @Recolectores,
        Turno = @Turno,
        Fecha_Unica = @Fecha_Unica,
        Ruta = @Ruta,
        Costo_Tonelada = @Costo_Tonelada,
        Costo_Disposicion = @Costo_Disposicion
    WHERE ID = @ID;
END
GO

-- 4. Asegurar que el SP de lectura devuelva Fecha_Unica para mostrarla en tablas/PDFs.
IF OBJECT_ID('SP_ServiciosCotizados_GetByTrato', 'P') IS NOT NULL
BEGIN
    DECLARE @sqlGet NVARCHAR(MAX);
    SELECT @sqlGet = OBJECT_DEFINITION(OBJECT_ID('SP_ServiciosCotizados_GetByTrato'));
    IF @sqlGet IS NOT NULL AND @sqlGet NOT LIKE '%Fecha_Unica%'
    BEGIN
        RAISERROR ('ADVERTENCIA: SP_ServiciosCotizados_GetByTrato no devuelve Fecha_Unica. Revisar el SP para incluir la columna en el SELECT.', 0, 1) WITH NOWAIT;
    END
END
GO
