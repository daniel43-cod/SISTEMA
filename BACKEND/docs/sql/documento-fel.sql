-- Crear antes de utilizar los futuros endpoints FEL. No envía documentos a INFILE.
-- Si la tabla ya existe, no modifica su estructura.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID('dbo.documento_fel', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.documento_fel (
        id_documento_fel int IDENTITY(1,1) NOT NULL PRIMARY KEY,
        id_venta int NOT NULL,
        id_usuario int NOT NULL,
        ambiente varchar(10) NOT NULL,
        tipo_dte varchar(4) NOT NULL,
        estado varchar(15) NOT NULL,
        identificador uniqueidentifier NOT NULL,
        uuid varchar(36) NULL,
        serie varchar(8) NULL,
        numero varchar(30) NULL,
        fecha_creacion datetimeoffset NOT NULL,
        fecha_emision datetimeoffset NOT NULL,
        fecha_certificacion datetimeoffset NULL,
        xml_enviado nvarchar(max) NOT NULL,
        xml_certificado nvarchar(max) NULL,
        respuesta nvarchar(max) NULL,
        version rowversion NOT NULL,
        CONSTRAINT FK_documento_fel_ventas_id_venta FOREIGN KEY (id_venta) REFERENCES dbo.ventas(id_ventas),
        CONSTRAINT FK_documento_fel_usuario_id_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario(id_usuario),
        CONSTRAINT CK_documento_fel_ambiente CHECK (ambiente IN ('PRUEBAS', 'PRODUCCION')),
        CONSTRAINT CK_documento_fel_tipo CHECK (tipo_dte = 'FACT'),
        CONSTRAINT CK_documento_fel_estado CHECK (estado IN ('PENDIENTE', 'EN_PROCESO', 'CERTIFICADA', 'RECHAZADA', 'POR_CONFIRMAR')),
        CONSTRAINT CK_documento_fel_certificada CHECK (estado <> 'CERTIFICADA' OR
            (uuid IS NOT NULL AND serie IS NOT NULL AND numero IS NOT NULL AND xml_certificado IS NOT NULL AND fecha_certificacion IS NOT NULL))
    );
    CREATE UNIQUE INDEX UX_documento_fel_venta_ambiente ON dbo.documento_fel(id_venta, ambiente);
    CREATE UNIQUE INDEX UX_documento_fel_identificador ON dbo.documento_fel(identificador);
    CREATE UNIQUE INDEX UX_documento_fel_uuid ON dbo.documento_fel(uuid) WHERE uuid IS NOT NULL;
    CREATE INDEX IX_documento_fel_usuario ON dbo.documento_fel(id_usuario);
END;
COMMIT TRANSACTION;
