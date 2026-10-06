-- Ejecutar en SQL Server sobre la base SISTEMA.
-- Amplía la columna para guardar las URLs admitidas por la API.
USE [SISTEMA];
GO

IF COL_LENGTH('dbo.productos', 'imagen') IS NULL
    THROW 50001, 'No existe dbo.productos.imagen. Verifica la base seleccionada.', 1;

-- No reducir una columna que ya admite más caracteres o es de tamaño MAX.
IF EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('dbo.productos') AND c.name = 'imagen'
      AND t.name IN ('varchar', 'nvarchar')
      AND c.max_length <> -1
      AND c.max_length / CASE WHEN t.name = 'nvarchar' THEN 2 ELSE 1 END < 2048
)
BEGIN
    ALTER TABLE dbo.productos ALTER COLUMN imagen nvarchar(2048) NULL;
END;
