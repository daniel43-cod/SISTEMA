-- Ejecutar en la base del sistema con la API detenida, antes de habilitar caja compartida.
-- No borra ni cierra sesiones existentes: los duplicados requieren revision manual.
SET XACT_ABORT ON;
BEGIN TRY
    BEGIN TRANSACTION;
    IF (SELECT COUNT_BIG(*) FROM dbo.sesion_caja WITH (TABLOCKX, HOLDLOCK)
        WHERE fecha_cierre IS NULL) > 1
        THROW 50001, 'Existen varias sesiones abiertas. Revisalas antes de aplicar el indice.', 1;

    -- En SQL Server, una clave UNIQUE nullable admite un solo NULL.
    -- El filtro excluye sesiones cerradas y garantiza una unica abierta global.
    IF NOT EXISTS (SELECT 1 FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.sesion_caja') AND name = N'UX_sesion_caja_unica_abierta')
        CREATE UNIQUE INDEX UX_sesion_caja_unica_abierta
            ON dbo.sesion_caja(fecha_cierre) WHERE fecha_cierre IS NULL;
    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    THROW;
END CATCH;
