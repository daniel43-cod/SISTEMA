-- Ejecutar antes de desplegar el backend de abonos idempotentes.
-- Los pagos históricos y los pagos iniciales de ventas conservan NULL.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.pagos', 'id_operacion') IS NULL
    ALTER TABLE dbo.pagos ADD id_operacion uniqueidentifier NULL;
COMMIT;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.pagos') AND name = 'UX_pagos_id_operacion')
    CREATE UNIQUE INDEX UX_pagos_id_operacion ON dbo.pagos(id_operacion) WHERE id_operacion IS NOT NULL;
GO