/*
   SQL Server 2016+; compatibilidad de base >= 130 (OPENJSON).
   Ejecutar en la base del sistema, con el backend detenido y respaldo previo.
   Crea intentos y mensajes normalizados y copia las respuestas JSON existentes.
   NO elimina respuesta ni XML: el backend actual todavía utiliza esas columnas.
   Después se debe adaptar el backend para escribir estas tablas y verificar la migración.
   El historial anterior solo contiene la última respuesta conservada por documento;
   no es posible reconstruir envíos que ya fueron sobrescritos.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID('dbo.documento_fel', 'U') IS NULL
    THROW 50001, 'Primero debe existir dbo.documento_fel.', 1;
IF (SELECT compatibility_level FROM sys.databases WHERE name = DB_NAME()) < 130
    THROW 50002, 'Se requiere nivel de compatibilidad 130 o superior para OPENJSON.', 1;

BEGIN TRY
    BEGIN TRANSACTION;
    -- Evitar que otro proceso cambie las respuestas mientras se copian.
    DECLARE @cantidad_documentos bigint;
    SELECT @cantidad_documentos = COUNT_BIG(*) FROM dbo.documento_fel WITH (TABLOCKX, HOLDLOCK);

    IF OBJECT_ID('dbo.documento_fel_intento', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.documento_fel_intento (
            id_intento bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_documento_fel_intento PRIMARY KEY,
            id_documento_fel int NOT NULL,
            id_usuario int NOT NULL,
            identificador uniqueidentifier NOT NULL,
            estado varchar(15) NOT NULL,
            fecha_creacion datetimeoffset NOT NULL,
            fecha_emision datetimeoffset NOT NULL,
            fecha_respuesta datetimeoffset NULL,
            codigo_http smallint NULL, -- No disponible en el historial actual.
            resultado bit NULL,
            origen nvarchar(max) NULL,
            descripcion nvarchar(max) NULL,
            uuid_respuesta nvarchar(max) NULL,
            serie_respuesta nvarchar(max) NULL,
            numero_respuesta nvarchar(max) NULL,
            saldo bigint NULL,
            creditos bigint NULL,
            cupo_diario bigint NULL,
            saldo_diario bigint NULL,
            alertas_infile bit NULL,
            alertas_sat bit NULL,
            cantidad_errores int NULL,
            xml_enviado nvarchar(max) NOT NULL,
            xml_certificado nvarchar(max) NULL,
            migrado bit NOT NULL CONSTRAINT DF_fel_intento_migrado DEFAULT (0),
            version rowversion NOT NULL,
            CONSTRAINT FK_fel_intento_documento FOREIGN KEY (id_documento_fel)
                REFERENCES dbo.documento_fel(id_documento_fel),
            CONSTRAINT FK_fel_intento_usuario FOREIGN KEY (id_usuario) REFERENCES dbo.usuario(id_usuario),
            CONSTRAINT UQ_fel_intento_identificador UNIQUE (identificador),
            CONSTRAINT CK_fel_intento_estado CHECK
                (estado IN ('PENDIENTE','EN_PROCESO','CERTIFICADA','RECHAZADA','POR_CONFIRMAR')),
            CONSTRAINT CK_fel_intento_http CHECK (codigo_http BETWEEN 100 AND 599)
        );
        CREATE INDEX IX_fel_intento_documento_fecha
            ON dbo.documento_fel_intento(id_documento_fel, fecha_creacion, id_intento);
        CREATE INDEX IX_fel_intento_usuario ON dbo.documento_fel_intento(id_usuario);
    END;

    IF OBJECT_ID('dbo.documento_fel_error', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.documento_fel_error (
            id_error bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_documento_fel_error PRIMARY KEY,
            id_intento bigint NOT NULL,
            orden int NOT NULL,
            resultado bit NULL,
            fuente nvarchar(max) NULL,
            categoria nvarchar(max) NULL,
            numeral nvarchar(max) NULL,
            validacion nvarchar(max) NULL,
            mensaje nvarchar(max) NULL,
            CONSTRAINT FK_fel_error_intento FOREIGN KEY (id_intento)
                REFERENCES dbo.documento_fel_intento(id_intento),
            CONSTRAINT UQ_fel_error_orden UNIQUE (id_intento, orden)
        );
    END;

    IF OBJECT_ID('dbo.documento_fel_alerta', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.documento_fel_alerta (
            id_alerta bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_documento_fel_alerta PRIMARY KEY,
            id_intento bigint NOT NULL,
            origen varchar(6) NOT NULL,
            orden int NOT NULL,
            fuente nvarchar(max) NULL,
            categoria nvarchar(max) NULL,
            numeral nvarchar(max) NULL,
            validacion nvarchar(max) NULL,
            mensaje nvarchar(max) NULL,
            CONSTRAINT FK_fel_alerta_intento FOREIGN KEY (id_intento)
                REFERENCES dbo.documento_fel_intento(id_intento),
            CONSTRAINT CK_fel_alerta_origen CHECK (origen IN ('INFILE','SAT')),
            CONSTRAINT UQ_fel_alerta_orden UNIQUE (id_intento, origen, orden)
        );
    END;

    -- Solo se migran documentos con una respuesta JSON objeto válida.
    -- El JSON original conserva campos adicionales o formas no reconocidas hasta su revisión.
    DECLARE @nuevos TABLE (id_intento bigint, id_documento_fel int);
    INSERT dbo.documento_fel_intento (
        id_documento_fel, id_usuario, identificador, estado, fecha_creacion, fecha_emision,
        fecha_respuesta, resultado, origen, descripcion, uuid_respuesta, serie_respuesta,
        numero_respuesta, saldo, creditos, cupo_diario, saldo_diario,
        alertas_infile, alertas_sat, cantidad_errores, xml_enviado, xml_certificado, migrado)
    OUTPUT inserted.id_intento, inserted.id_documento_fel INTO @nuevos
    SELECT d.id_documento_fel, d.id_usuario, d.identificador, d.estado, d.fecha_creacion, d.fecha_emision,
        TRY_CONVERT(datetimeoffset, j.fecha),
        CASE j.resultado WHEN 'true' THEN 1 WHEN 'false' THEN 0 END,
        j.origen, j.descripcion, j.uuid, j.serie, j.numero,
        TRY_CONVERT(bigint, COALESCE(j.saldo, JSON_VALUE(j.control, '$.Saldo'))),
        TRY_CONVERT(bigint, COALESCE(j.creditos, JSON_VALUE(j.control, '$.Creditos'))),
        TRY_CONVERT(bigint, JSON_VALUE(j.control, '$.cupo_diario')),
        TRY_CONVERT(bigint, JSON_VALUE(j.control, '$.saldo_diario')),
        CASE j.alertas_infile WHEN 'true' THEN 1 WHEN 'false' THEN 0 END,
        CASE j.alertas_sat WHEN 'true' THEN 1 WHEN 'false' THEN 0 END,
        TRY_CONVERT(int, j.cantidad_errores), d.xml_enviado, d.xml_certificado, 1
    FROM dbo.documento_fel d
    CROSS APPLY OPENJSON(CASE WHEN ISJSON(d.respuesta) = 1 AND LEFT(LTRIM(d.respuesta),1) = '{'
        THEN d.respuesta ELSE N'{}' END)
    WITH (
        fecha nvarchar(100) '$.fecha', resultado nvarchar(10) '$.resultado',
        origen nvarchar(max) '$.origen', descripcion nvarchar(max) '$.descripcion',
        uuid nvarchar(max) '$.uuid', serie nvarchar(max) '$.serie', numero nvarchar(max) '$.numero',
        saldo nvarchar(100) '$.saldo', creditos nvarchar(100) '$.creditos',
        control nvarchar(max) '$.control_emision' AS JSON,
        alertas_infile nvarchar(10) '$.alertas_infile', alertas_sat nvarchar(10) '$.alertas_sat',
        cantidad_errores nvarchar(100) '$.cantidad_errores'
    ) j
    WHERE ISJSON(d.respuesta) = 1 AND LEFT(LTRIM(d.respuesta),1) = '{'
      AND NOT EXISTS (SELECT 1 FROM dbo.documento_fel_intento i WHERE i.identificador = d.identificador);

    INSERT dbo.documento_fel_error(id_intento, orden, resultado, fuente, categoria, numeral, validacion, mensaje)
    SELECT n.id_intento, CONVERT(int, e.[key]),
        CASE p.resultado WHEN 'true' THEN 1 WHEN 'false' THEN 0 END,
        p.fuente, p.categoria, p.numeral, p.validacion,
        CASE WHEN e.[type] = 1 THEN e.value ELSE COALESCE(p.mensaje_error, p.mensaje, p.descripcion) END
    FROM @nuevos n
    JOIN dbo.documento_fel d ON d.id_documento_fel = n.id_documento_fel
    CROSS APPLY OPENJSON(d.respuesta, '$.descripcion_errores') e
    OUTER APPLY OPENJSON(CASE WHEN e.[type] = 5 THEN e.value ELSE N'{}' END)
    WITH (resultado nvarchar(10), fuente nvarchar(max), categoria nvarchar(max), numeral nvarchar(max),
        validacion nvarchar(max), mensaje_error nvarchar(max), mensaje nvarchar(max), descripcion nvarchar(max)) p;

    INSERT dbo.documento_fel_alerta(id_intento, origen, orden, fuente, categoria, numeral, validacion, mensaje)
    SELECT n.id_intento, a.origen, CONVERT(int, e.[key]), p.fuente, p.categoria, p.numeral, p.validacion,
        CASE WHEN e.[type] = 1 THEN e.value ELSE COALESCE(p.mensaje_error, p.mensaje, p.descripcion) END
    FROM @nuevos n
    JOIN dbo.documento_fel d ON d.id_documento_fel = n.id_documento_fel
    CROSS APPLY (VALUES
        ('INFILE', JSON_QUERY(d.respuesta, '$.descripcion_alertas_infile')),
        ('SAT', JSON_QUERY(d.respuesta, '$.descripcion_alertas_sat'))
    ) a(origen, contenido)
    CROSS APPLY OPENJSON(COALESCE(a.contenido, N'[]')) e
    OUTER APPLY OPENJSON(CASE WHEN e.[type] = 5 THEN e.value ELSE N'{}' END)
    WITH (fuente nvarchar(max), categoria nvarchar(max), numeral nvarchar(max), validacion nvarchar(max),
        mensaje_error nvarchar(max), mensaje nvarchar(max), descripcion nvarchar(max)) p;

    COMMIT TRANSACTION;
    SELECT COUNT(*) AS intentos_migrados_en_esta_ejecucion FROM @nuevos;
    -- Revisar estas respuestas: no fueron migradas automáticamente.
    SELECT id_documento_fel, estado AS estado_documento, N'Respuesta no JSON objeto' AS observacion
    FROM dbo.documento_fel
    WHERE respuesta IS NOT NULL
      AND (ISJSON(respuesta) <> 1 OR LEFT(LTRIM(respuesta),1) <> '{');
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
