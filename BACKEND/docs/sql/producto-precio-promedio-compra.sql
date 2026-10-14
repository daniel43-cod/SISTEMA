-- Ejecutar en la base del sistema después de renombrar la columna a precio_promedio_compra.
-- Los costos por unidad necesitan más precisión que los totales de compra.
IF COL_LENGTH('dbo.productos', 'precio_promedio_compra') IS NULL
    THROW 50001, 'Falta la columna precio_promedio_compra en dbo.productos.', 1;

ALTER TABLE dbo.productos
    ALTER COLUMN precio_promedio_compra decimal(18,6) NULL;
