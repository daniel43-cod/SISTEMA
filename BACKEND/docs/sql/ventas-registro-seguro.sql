-- PREPARADO PARA REVISION. NO EJECUTADO POR CODEX.
-- SQL Server. Hacer respaldo y verificar el esquema real antes de desplegar la API.
-- No se reconstruyen precios/costos históricos a partir de los precios actuales.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'dbo.ventas', N'U') IS NULL OR OBJECT_ID(N'dbo.detalle_venta', N'U') IS NULL
    THROW 50001, 'Verifica los nombres reales de las tablas ventas y detalle_venta antes de aplicar.', 1;

IF COL_LENGTH(N'dbo.detalle_venta', N'costo_unitario_venta') IS NULL
    ALTER TABLE dbo.detalle_venta ADD costo_unitario_venta decimal(18,6) NULL;

-- Conservar seis decimales para ganancias calculadas con el costo promedio.
ALTER TABLE dbo.detalle_venta ALTER COLUMN ganancia decimal(18,6) NOT NULL;
ALTER TABLE dbo.ventas ALTER COLUMN ganancia_total decimal(18,6) NOT NULL;

IF OBJECT_ID(N'dbo.venta_operacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.venta_operacion (
        id_operacion uniqueidentifier NOT NULL CONSTRAINT PK_venta_operacion PRIMARY KEY,
        id_usuario int NOT NULL,
        huella varchar(64) NOT NULL,
        id_venta int NOT NULL,
        CONSTRAINT FK_venta_operacion_ventas FOREIGN KEY(id_venta) REFERENCES dbo.ventas(id_ventas)
    );
    CREATE UNIQUE INDEX IX_venta_operacion_id_venta ON dbo.venta_operacion(id_venta);
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.ventas') AND name = N'IX_ventas_id_sesion_caja_fecha_venta')
    CREATE INDEX IX_ventas_id_sesion_caja_fecha_venta ON dbo.ventas(id_sesion_caja, fecha_venta);
COMMIT TRANSACTION;
