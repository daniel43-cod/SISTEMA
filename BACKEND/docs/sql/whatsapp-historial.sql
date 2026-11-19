-- Instalación nueva: cada Compartir crea un envío. No envía mensajes ni modifica ventas.
-- Para actualizar tablas existentes, aplicar también whatsapp-compartir-siempre.sql.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.whatsapp_envio', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.whatsapp_envio (
        id_envio uniqueidentifier NOT NULL PRIMARY KEY,
        id_venta int NOT NULL REFERENCES dbo.ventas(id_ventas),
        id_empresa int NOT NULL REFERENCES dbo.empresa(id_empresa),
        id_usuario int NOT NULL REFERENCES dbo.usuario(id_usuario),
        telefono varchar(15) NOT NULL,
        plantilla varchar(512) NOT NULL,
        idioma varchar(10) NOT NULL,
        remitente varchar(30) NOT NULL,
        creado_utc datetime2 NOT NULL
    );
    CREATE INDEX IX_whatsapp_envio_venta_empresa ON dbo.whatsapp_envio(id_venta, id_empresa);
END;
IF OBJECT_ID('dbo.whatsapp_envio_pagina', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.whatsapp_envio_pagina (
        id_envio uniqueidentifier NOT NULL REFERENCES dbo.whatsapp_envio(id_envio),
        pagina int NOT NULL CHECK (pagina > 0),
        estado varchar(20) NOT NULL,
        message_id varchar(512) NULL,
        etapa varchar(20) NULL,
        codigo_meta int NULL,
        actualizado_utc datetime2 NULL,
        PRIMARY KEY (id_envio, pagina),
        CHECK (estado IN ('no_intentado','subiendo','enviando','aceptado','rechazado','fallido','desconocido'))
    );
END;
IF OBJECT_ID('dbo.whatsapp_envio_intento', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.whatsapp_envio_intento (
        id_intento bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        id_envio uniqueidentifier NOT NULL,
        pagina int NOT NULL,
        id_usuario int NOT NULL REFERENCES dbo.usuario(id_usuario),
        inicio_utc datetime2 NOT NULL,
        fin_utc datetime2 NULL,
        estado varchar(20) NOT NULL,
        message_id varchar(512) NULL,
        etapa varchar(20) NULL,
        codigo_meta int NULL,
        FOREIGN KEY (id_envio, pagina) REFERENCES dbo.whatsapp_envio_pagina(id_envio, pagina),
        CHECK (estado IN ('subiendo','enviando','aceptado','rechazado','fallido','desconocido'))
    );
    CREATE INDEX IX_whatsapp_envio_intento_message_id ON dbo.whatsapp_envio_intento(message_id) WHERE message_id IS NOT NULL;
END;
COMMIT TRANSACTION;
