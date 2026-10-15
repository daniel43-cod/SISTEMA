-- Ejecutar antes de usar el listado de compras de la sesión activa.
-- Las compras anteriores quedan sin asignar; vincularlas solo con información verificada.
IF COL_LENGTH('dbo.registro_compras', 'id_sesion_caja') IS NULL
BEGIN
    ALTER TABLE dbo.registro_compras ADD id_sesion_caja int NULL;
END;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.registro_compras') AND name = 'IX_registro_compras_sesion')
BEGIN
    CREATE INDEX IX_registro_compras_sesion
        ON dbo.registro_compras(id_sesion_caja, id_compra);
END;
