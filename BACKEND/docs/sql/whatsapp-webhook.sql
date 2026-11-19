-- Aplicar después de whatsapp-historial.sql. No modifica ventas ni dispara envíos.
SET XACT_ABORT ON;
IF OBJECT_ID('dbo.whatsapp_webhook_evento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.whatsapp_webhook_evento (
        huella varchar(64) NOT NULL PRIMARY KEY,
        cuenta varchar(30) NOT NULL,
        remitente varchar(30) NOT NULL,
        message_id varchar(512) NOT NULL,
        estado varchar(15) NOT NULL CHECK (estado IN ('sent','delivered','read','failed')),
        timestamp_meta bigint NOT NULL CHECK (timestamp_meta >= 0),
        codigo_meta int NULL,
        recibido_utc datetime2 NOT NULL
    );
    CREATE INDEX IX_whatsapp_webhook_message_id ON dbo.whatsapp_webhook_evento(message_id);
END;
