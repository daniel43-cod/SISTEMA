-- Ejecutar en la base de datos del sistema si aún no existe la columna.
IF COL_LENGTH('dbo.categorias', 'url_imagen') IS NULL
BEGIN
    ALTER TABLE dbo.categorias ADD url_imagen nvarchar(2048) NULL;
END;
