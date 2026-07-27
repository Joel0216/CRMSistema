-- ============================================================
-- Seed de catálogos para Cotizador / Contratos
-- Rellena las tablas de catálogo para que RSU y RME aparezcan
-- al agregar servicios. NO contiene SQL incrustado en código C#;
-- solo datos de base de datos.
--
-- IMPORTANTE: Ejecutar con sqlcmd usando UTF-8, por ejemplo:
--   sqlcmd -S localhost -d CRM_Base -i seed_catalogos_crm.sql -f 65001
-- ============================================================
:setvar DatabaseName "CRM_Base"
GO
USE $(DatabaseName);
GO

SET NOCOUNT ON;

-- ------------------------------------------------------------
-- 1. Unidades RME (crm_configurador_unidades)
-- ------------------------------------------------------------
IF OBJECT_ID('crm_configurador_unidades', 'U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM crm_configurador_unidades WHERE Nombre_Unidad = N'Camión 3.5 Ton')
        INSERT INTO crm_configurador_unidades (Nombre_Unidad, Modalidad, Capacidad_Toneladas, Costo_Unitario, Estatus)
        VALUES (N'Camión 3.5 Ton', N'Viaje', 3.5, 2500.00, 1);

    IF NOT EXISTS (SELECT 1 FROM crm_configurador_unidades WHERE Nombre_Unidad = N'Camión 7 Ton')
        INSERT INTO crm_configurador_unidades (Nombre_Unidad, Modalidad, Capacidad_Toneladas, Costo_Unitario, Estatus)
        VALUES (N'Camión 7 Ton', N'Viaje', 7.0, 4500.00, 1);

    IF NOT EXISTS (SELECT 1 FROM crm_configurador_unidades WHERE Nombre_Unidad = N'Camión 14 Ton')
        INSERT INTO crm_configurador_unidades (Nombre_Unidad, Modalidad, Capacidad_Toneladas, Costo_Unitario, Estatus)
        VALUES (N'Camión 14 Ton', N'Viaje', 14.0, 8500.00, 1);

    IF NOT EXISTS (SELECT 1 FROM crm_configurador_unidades WHERE Nombre_Unidad = N'Camión de Caja')
        INSERT INTO crm_configurador_unidades (Nombre_Unidad, Modalidad, Capacidad_Toneladas, Costo_Unitario, Estatus)
        VALUES (N'Camión de Caja', N'Viaje', 5.0, 5500.00, 1);

    IF NOT EXISTS (SELECT 1 FROM crm_configurador_unidades WHERE Nombre_Unidad = N'Pipote')
        INSERT INTO crm_configurador_unidades (Nombre_Unidad, Modalidad, Capacidad_Toneladas, Costo_Unitario, Estatus)
        VALUES (N'Pipote', N'Peso', 1.5, 1800.00, 1);
END
GO

-- ------------------------------------------------------------
-- 2. Servicios de residuos (servicios_residuos)
-- RSU con precio unitario por bolsa; RME como opciones de manejo.
-- ------------------------------------------------------------
IF OBJECT_ID('servicios_residuos', 'U') IS NOT NULL
BEGIN
    -- RSU
    IF NOT EXISTS (SELECT 1 FROM servicios_residuos WHERE codigo_sana = N'RSU-001')
        INSERT INTO servicios_residuos (codigo_control, codigo_sana, tipo, descripcion, precio, unidad_medida, activo)
        VALUES (N'RSU-001', N'RSU-001', N'Residuos Sólidos Urbanos', N'Basura General', 18.60, N'Contenedor', N'Si');
    ELSE
        UPDATE servicios_residuos SET descripcion = N'Basura General', precio = 18.60, unidad_medida = N'Contenedor', activo = N'Si' WHERE codigo_sana = N'RSU-001';

    IF NOT EXISTS (SELECT 1 FROM servicios_residuos WHERE codigo_sana = N'RSU-002')
        INSERT INTO servicios_residuos (codigo_control, codigo_sana, tipo, descripcion, precio, unidad_medida, activo)
        VALUES (N'RSU-002', N'RSU-002', N'Residuos Sólidos Urbanos', N'Residuos Orgánicos', 18.60, N'Bolsa', N'Si');
    ELSE
        UPDATE servicios_residuos SET descripcion = N'Residuos Orgánicos', precio = 18.60, unidad_medida = N'Bolsa', activo = N'Si' WHERE codigo_sana = N'RSU-002';

    IF NOT EXISTS (SELECT 1 FROM servicios_residuos WHERE codigo_sana = N'RSU-003')
        INSERT INTO servicios_residuos (codigo_control, codigo_sana, tipo, descripcion, precio, unidad_medida, activo)
        VALUES (N'RSU-003', N'RSU-003', N'Residuos Sólidos Urbanos', N'Manejo de Reciclables', 18.60, N'Bolsa', N'Si');
    ELSE
        UPDATE servicios_residuos SET descripcion = N'Manejo de Reciclables', precio = 18.60, unidad_medida = N'Bolsa', activo = N'Si' WHERE codigo_sana = N'RSU-003';

    -- RME
    IF NOT EXISTS (SELECT 1 FROM servicios_residuos WHERE codigo_sana = N'RME-001')
        INSERT INTO servicios_residuos (codigo_control, codigo_sana, tipo, descripcion, precio, unidad_medida, activo)
        VALUES (N'RME-001', N'RME-001', N'Residuos de Manejo Especial', N'Recolección de Cartón', 0, N'Tonelada', N'Si');
    ELSE
        UPDATE servicios_residuos SET descripcion = N'Recolección de Cartón', unidad_medida = N'Tonelada', activo = N'Si' WHERE codigo_sana = N'RME-001';

    IF NOT EXISTS (SELECT 1 FROM servicios_residuos WHERE codigo_sana = N'RME-002')
        INSERT INTO servicios_residuos (codigo_control, codigo_sana, tipo, descripcion, precio, unidad_medida, activo)
        VALUES (N'RME-002', N'RME-002', N'Residuos de Manejo Especial', N'Manejo de Aceites', 0, N'Litro', N'Si');
    ELSE
        UPDATE servicios_residuos SET descripcion = N'Manejo de Aceites', unidad_medida = N'Litro', activo = N'Si' WHERE codigo_sana = N'RME-002';

    IF NOT EXISTS (SELECT 1 FROM servicios_residuos WHERE codigo_sana = N'RME-003')
        INSERT INTO servicios_residuos (codigo_control, codigo_sana, tipo, descripcion, precio, unidad_medida, activo)
        VALUES (N'RME-003', N'RME-003', N'Residuos de Manejo Especial', N'Manejo de Plásticos Industriales', 0, N'Tonelada', N'Si');
    ELSE
        UPDATE servicios_residuos SET descripcion = N'Manejo de Plásticos Industriales', unidad_medida = N'Tonelada', activo = N'Si' WHERE codigo_sana = N'RME-003';

    IF NOT EXISTS (SELECT 1 FROM servicios_residuos WHERE codigo_sana = N'RME-004')
        INSERT INTO servicios_residuos (codigo_control, codigo_sana, tipo, descripcion, precio, unidad_medida, activo)
        VALUES (N'RME-004', N'RME-004', N'Residuos de Manejo Especial', N'Residuos Peligrosos', 0, N'Litro', N'Si');
    ELSE
        UPDATE servicios_residuos SET descripcion = N'Residuos Peligrosos', unidad_medida = N'Litro', activo = N'Si' WHERE codigo_sana = N'RME-004';
END
GO

PRINT N'Catálogos de RME y RSU sembrados correctamente.';
GO

