-- Paso 1: columnas para recuperar certificaciones interrumpidas.
-- Ejecutar en la base del sistema antes de iniciar el backend actualizado.
-- No cambia estados, no genera identificadores FEL y no envía nada a INFILE.
-- Las fechas y el token de intentos anteriores quedan NULL: no se inventa su vigencia.
SET XACT_ABORT ON;
IF OBJECT_ID('dbo.documento_fel_intento', 'U') IS NULL
    THROW 50001, 'Primero ejecuta documento-fel-normalizar-respuesta.sql.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    IF COL_LENGTH('dbo.documento_fel_intento', 'procesamiento_inicio') IS NULL
        ALTER TABLE dbo.documento_fel_intento ADD procesamiento_inicio datetimeoffset NULL;
    IF COL_LENGTH('dbo.documento_fel_intento', 'procesamiento_vence') IS NULL
        ALTER TABLE dbo.documento_fel_intento ADD procesamiento_vence datetimeoffset NULL;
    IF COL_LENGTH('dbo.documento_fel_intento', 'procesamiento_token') IS NULL
        ALTER TABLE dbo.documento_fel_intento ADD procesamiento_token uniqueidentifier NULL;
    IF COL_LENGTH('dbo.documento_fel_intento', 'recuperaciones') IS NULL
        ALTER TABLE dbo.documento_fel_intento ADD recuperaciones int NOT NULL
            CONSTRAINT DF_fel_intento_recuperaciones DEFAULT (0) WITH VALUES;
    IF COL_LENGTH('dbo.documento_fel_intento', 'ultima_recuperacion') IS NULL
        ALTER TABLE dbo.documento_fel_intento ADD ultima_recuperacion datetimeoffset NULL;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.documento_fel_intento') AND name = 'IX_fel_intento_procesamiento_vence')
        EXEC(N'CREATE INDEX IX_fel_intento_procesamiento_vence
            ON dbo.documento_fel_intento(procesamiento_vence)
            WHERE estado = ''EN_PROCESO'';');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
