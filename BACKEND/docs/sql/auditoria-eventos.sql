-- Ejecutar manualmente en SQL Server, base SISTEMA.
-- Crea el almacenamiento; el backend deberá registrar los eventos.
-- Compatible con SQL Server 2016 o posterior (ISJSON).
USE [SISTEMA];
GO
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.auditoria_evento', N'U') IS NOT NULL
BEGIN
    PRINT N'dbo.auditoria_evento ya existe. No se modificó su estructura.';
    RETURN;
END;

IF OBJECT_ID(N'dbo.usuario', N'U') IS NULL
    THROW 50001, 'No existe dbo.usuario. Verifica la base seleccionada.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.auditoria_evento
    (
        id_auditoria bigint IDENTITY(1,1) NOT NULL,
        fecha_utc datetime2(7) NOT NULL
            CONSTRAINT DF_auditoria_evento_fecha DEFAULT SYSUTCDATETIME(),

        -- Responsable autenticado; NULL para intentos anónimos o tareas automáticas.
        id_usuario int NULL,
        -- Copia del identificador de usuario para interpretar el historial si cambia.
        usuario_responsable nvarchar(100) NULL,
        -- Cuenta objetivo en un intento fallido; NO identifica a un responsable autenticado.
        identificador_intentado nvarchar(100) NULL,

        -- Ej.: SESION_INICIADA, SESION_CERRADA, LOGIN_FALLIDO, PRODUCTO_CREADO,
        -- MARCA_EDITADA, CATEGORIA_DESACTIVADA, PRECIO_CAMBIADO, CAJA_ABIERTA.
        accion varchar(100) NOT NULL,
        entidad varchar(100) NULL,
        -- Texto para admitir IDs enteros, GUID u otros identificadores futuros.
        id_registro nvarchar(100) NULL,
        resultado varchar(12) NOT NULL,
        origen varchar(20) NOT NULL
            CONSTRAINT DF_auditoria_evento_origen DEFAULT 'API',

        -- Guardar únicamente campos permitidos; nunca serializar la entidad completa.
        datos_anteriores nvarchar(max) NULL,
        datos_nuevos nvarchar(max) NULL,
        motivo nvarchar(500) NULL,
        trace_id varchar(128) NULL,
        -- IPv4 o IPv6. El backend debe considerar solo proxies configurados como confiables.
        direccion_ip varchar(45) NULL,

        CONSTRAINT PK_auditoria_evento PRIMARY KEY CLUSTERED (id_auditoria),
        CONSTRAINT FK_auditoria_evento_usuario FOREIGN KEY (id_usuario)
            REFERENCES dbo.usuario(id_usuario) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_auditoria_evento_accion
            CHECK (LEN(LTRIM(RTRIM(accion))) > 0),
        CONSTRAINT CK_auditoria_evento_resultado
            CHECK (resultado IN ('EXITOSO', 'RECHAZADO', 'FALLIDO')),
        CONSTRAINT CK_auditoria_evento_origen
            CHECK (origen IN ('API', 'SISTEMA', 'SQL')),
        CONSTRAINT CK_auditoria_evento_anteriores_json
            CHECK (datos_anteriores IS NULL OR ISJSON(datos_anteriores) = 1),
        CONSTRAINT CK_auditoria_evento_nuevos_json
            CHECK (datos_nuevos IS NULL OR ISJSON(datos_nuevos) = 1),
        CONSTRAINT CK_auditoria_evento_registro_entidad
            CHECK (id_registro IS NULL OR
                (entidad IS NOT NULL AND LEN(LTRIM(RTRIM(entidad))) > 0))
    );

    CREATE INDEX IX_auditoria_evento_fecha
        ON dbo.auditoria_evento(fecha_utc DESC, id_auditoria DESC);
    CREATE INDEX IX_auditoria_evento_usuario_fecha
        ON dbo.auditoria_evento(id_usuario, fecha_utc DESC)
        WHERE id_usuario IS NOT NULL;
    CREATE INDEX IX_auditoria_evento_entidad_registro_fecha
        ON dbo.auditoria_evento(entidad, id_registro, fecha_utc DESC)
        WHERE id_registro IS NOT NULL;
    CREATE INDEX IX_auditoria_evento_accion_fecha
        ON dbo.auditoria_evento(accion, fecha_utc DESC);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- IMPORTANTE:
-- No guardar contraseñas, hashes de contraseña, JWT, cookies, tokens ni conexiones.
-- Los usuarios con historial deben desactivarse; la FK impide borrarlos físicamente.
-- No se registran eventos automáticamente: faltan modelo EF y servicio del backend.
-- Restringir UPDATE/DELETE y cambios de esquema con una cuenta de aplicación de
-- privilegios mínimos; no usar db_owner. No se alteran permisos en este script porque
-- dependen del usuario/rol SQL real de cada instalación.
-- Esto no es almacenamiento inmutable frente a administradores de SQL Server.
