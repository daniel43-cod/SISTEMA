-- Ejecutar antes de iniciar el backend actualizado.
-- No asigna CF ni infiere documentos para ventas históricas.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.ventas', 'tipo_identificacion_receptor') IS NULL
    ALTER TABLE dbo.ventas ADD tipo_identificacion_receptor nvarchar(3) NULL;
IF COL_LENGTH('dbo.ventas', 'identificacion_receptor') IS NULL
    ALTER TABLE dbo.ventas ADD identificacion_receptor nvarchar(20) NULL;
IF COL_LENGTH('dbo.ventas', 'nombre_receptor') IS NULL
    ALTER TABLE dbo.ventas ADD nombre_receptor nvarchar(250) NULL;
COMMIT TRANSACTION;
