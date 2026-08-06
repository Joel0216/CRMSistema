-- 1. Asegurar que la columna Fecha_Unica exista en la tabla real.
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = 'Fecha_Unica'
      AND Object_ID = Object_ID(N'dbo.crm_servicios_cotizados')
)
BEGIN
    ALTER TABLE dbo.crm_servicios_cotizados
    ADD Fecha_Unica VARCHAR(20) NULL;
END
GO

-- 2. Corregir el SP de inserción para usar la tabla real dbo.crm_servicios_cotizados
--    y recibir todos los parámetros que envía el DAL.
IF OBJECT_ID('dbo.SP_ServiciosCotizados_Insert', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_ServiciosCotizados_Insert;
GO
CREATE PROCEDURE dbo.SP_ServiciosCotizados_Insert
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
    SET NOCOUNT ON;
    INSERT INTO dbo.crm_servicios_cotizados
    (
        trato_id, tipo_residuo, frecuencia, periodicidad_pago,
        volumen_estimado, precio_unitario, dias_asignados,
        porcentaje_adicional, porcentaje_descuento, sucursal_id,
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

-- 3. Corregir el SP de actualización para usar la tabla real dbo.crm_servicios_cotizados.
IF OBJECT_ID('dbo.SP_ServiciosCotizados_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_ServiciosCotizados_Update;
GO
CREATE PROCEDURE dbo.SP_ServiciosCotizados_Update
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
    SET NOCOUNT ON;
    UPDATE dbo.crm_servicios_cotizados
    SET tipo_residuo = @Tipo_Residuo,
        frecuencia = @Frecuencia,
        periodicidad_pago = @Periodicidad_Pago,
        volumen_estimado = @Volumen_Estimado,
        precio_unitario = @Precio_Unitario,
        dias_asignados = @Dias_Asignados,
        porcentaje_adicional = @Porcentaje_Adicional,
        porcentaje_descuento = @Porcentaje_Descuento,
        sucursal_id = @Sucursal_ID,
        Tipo_Unidad = @Tipo_Unidad,
        Tipo_Cobro = @Tipo_Cobro,
        Recolectores = @Recolectores,
        Turno = @Turno,
        Fecha_Unica = @Fecha_Unica,
        Ruta = @Ruta,
        Costo_Tonelada = @Costo_Tonelada,
        Costo_Disposicion = @Costo_Disposicion
    WHERE id = @ID;
END
GO

-- 4. Asegurar que el SP de lectura devuelva Fecha_Unica.
IF OBJECT_ID('dbo.SP_ServiciosCotizados_GetByTrato', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SP_ServiciosCotizados_GetByTrato;
GO
CREATE PROCEDURE dbo.SP_ServiciosCotizados_GetByTrato
    @Trato_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        id, trato_id, tipo_residuo, frecuencia, periodicidad_pago,
        volumen_estimado, precio_unitario, dias_asignados,
        porcentaje_adicional, porcentaje_descuento, sucursal_id,
        Tipo_Unidad, Tipo_Cobro, Recolectores, Turno, Ruta,
        Limpieza_Extra, Costo_Renta, Combustible, Recorrido_Servicio,
        Costo_Tonelada, Costo_Disposicion, Capacidad_Toneladas,
        Fecha_Unica, fecha_creacion
    FROM dbo.crm_servicios_cotizados
    WHERE trato_id = @Trato_ID;
END
GO
