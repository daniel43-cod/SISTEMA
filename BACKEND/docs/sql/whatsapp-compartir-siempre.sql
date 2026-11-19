-- Para bases donde ya se aplicó whatsapp-historial.sql en su versión anterior.
-- Cada acción Compartir crea un envío nuevo para la misma venta/empresa.
-- No elimina tablas, registros de envío ni eventos de Meta.
-- Sí elimina snapshot_json y todas las copias antiguas almacenadas en esa columna.
-- Detener el backend anterior antes de aplicar el script y ejecutar luego la versión nueva.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.whatsapp_envio', 'U') IS NULL
    THROW 50001, 'Primero aplique whatsapp-historial.sql.', 1;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.whatsapp_envio')
    AND name = 'UX_whatsapp_envio_venta_empresa')
    DROP INDEX UX_whatsapp_envio_venta_empresa ON dbo.whatsapp_envio;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.whatsapp_envio')
    AND name = 'IX_whatsapp_envio_venta_empresa')
    CREATE INDEX IX_whatsapp_envio_venta_empresa ON dbo.whatsapp_envio(id_venta, id_empresa);

-- La versión nueva no utiliza esta columna. Eliminar sus copias antiguas según lo autorizado.
-- Las columnas de bloqueo antiguas son nullable y dejan de utilizarse.
IF COL_LENGTH('dbo.whatsapp_envio', 'snapshot_json') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id = OBJECT_ID('dbo.whatsapp_envio') AND name = 'CK_whatsapp_snapshot')
        ALTER TABLE dbo.whatsapp_envio DROP CONSTRAINT CK_whatsapp_snapshot;
    EXEC(N'ALTER TABLE dbo.whatsapp_envio DROP COLUMN snapshot_json;');
END;
COMMIT TRANSACTION;
