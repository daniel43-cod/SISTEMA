-- Ejecutar antes de desplegar el backend actualizado.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.pagos_compra', 'id_operacion') IS NULL
    ALTER TABLE dbo.pagos_compra ADD id_operacion uniqueidentifier NULL;
COMMIT TRANSACTION;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.pagos_compra') AND name = 'UX_pagos_compra_id_operacion')
    CREATE UNIQUE INDEX UX_pagos_compra_id_operacion
    ON dbo.pagos_compra(id_operacion) WHERE id_operacion IS NOT NULL;
GO
