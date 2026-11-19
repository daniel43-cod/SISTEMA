-- Paso 1: preparar columnas para IVA con seis decimales.
-- Ejecutar antes de iniciar el backend actualizado.
-- No calcula ni recalcula impuestos; los detalles históricos quedan en NULL.
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.detalle_venta', 'U') IS NULL OR COL_LENGTH('dbo.ventas', 'impuesto') IS NULL
    THROW 50001, 'Deben existir detalle_venta y ventas.impuesto.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH('dbo.detalle_venta', 'impuesto') IS NULL
        ALTER TABLE dbo.detalle_venta ADD impuesto decimal(18,6) NULL;

    -- Evitar pérdida de valores si la columna actual tiene un rango o escala mayor.
    IF EXISTS (
        SELECT 1 FROM dbo.ventas WITH (TABLOCKX, HOLDLOCK)
        WHERE impuesto IS NOT NULL AND (
            TRY_CONVERT(decimal(18,6), impuesto) IS NULL
            OR impuesto <> TRY_CONVERT(decimal(18,6), impuesto)
        )
    )
        THROW 50002, 'Hay impuestos que no caben exactamente en decimal(18,6). Revisarlos antes de continuar.', 1;

    -- Conservar la nulabilidad que tiene la columna existente.
    IF EXISTS (SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('dbo.ventas') AND name = 'impuesto' AND is_nullable = 1)
        ALTER TABLE dbo.ventas ALTER COLUMN impuesto decimal(18,6) NULL;
    ELSE
        ALTER TABLE dbo.ventas ALTER COLUMN impuesto decimal(18,6) NOT NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
