-- Ejecutar en la base SISTEMA antes de usar el modelo actualizado.
USE [SISTEMA];
GO
IF COL_LENGTH('dbo.marcas', 'url_imagen') IS NULL
BEGIN
    ALTER TABLE dbo.marcas ADD url_imagen nvarchar(2048) NULL;
END;
