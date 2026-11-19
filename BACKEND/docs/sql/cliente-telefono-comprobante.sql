-- Solo si llegó a agregarse ventas.telefono_comprobante con el script anterior.
-- El código nuevo utiliza cliente.telefono, que ya existe; no agrega otra columna.
-- Detener el backend anterior antes de ejecutar. Puede repetirse sin efectos adicionales.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.ventas', 'telefono_comprobante') IS NOT NULL
BEGIN
    -- SQL dinámico para que también funcione cuando la columna ya no existe.
    EXEC(N'
        -- No elegir arbitrariamente entre números diferentes del mismo cliente.
        IF EXISTS (
            SELECT v.id_cliente FROM dbo.ventas v
            WHERE NULLIF(LTRIM(RTRIM(v.telefono_comprobante)), '''') IS NOT NULL
            GROUP BY v.id_cliente
            HAVING COUNT(DISTINCT LTRIM(RTRIM(v.telefono_comprobante))) > 1
        ) THROW 50001, ''Hay clientes con distintos teléfonos en sus ventas. Revise esos datos antes de eliminar la columna.'', 1;

        -- No sobrescribir un teléfono ya existente ni descartar otro sin revisión.
        IF EXISTS (
            SELECT 1 FROM dbo.ventas v JOIN dbo.cliente c ON c.id_cliente = v.id_cliente
            WHERE NULLIF(LTRIM(RTRIM(v.telefono_comprobante)), '''') IS NOT NULL
              AND NULLIF(LTRIM(RTRIM(c.telefono)), '''') IS NOT NULL
              AND LTRIM(RTRIM(c.telefono)) <> LTRIM(RTRIM(v.telefono_comprobante))
        ) THROW 50002, ''Hay teléfonos de venta diferentes al teléfono del cliente. Revise los datos; no se modificó ni eliminó la columna.'', 1;

        UPDATE c SET telefono = p.telefono
        FROM dbo.cliente c
        JOIN (
            SELECT id_cliente, MAX(LTRIM(RTRIM(telefono_comprobante))) AS telefono
            FROM dbo.ventas
            WHERE NULLIF(LTRIM(RTRIM(telefono_comprobante)), '''') IS NOT NULL
            GROUP BY id_cliente
        ) p ON p.id_cliente = c.id_cliente
        WHERE NULLIF(LTRIM(RTRIM(c.telefono)), '''') IS NULL;

        ALTER TABLE dbo.ventas DROP COLUMN telefono_comprobante;
    ');
END;
COMMIT TRANSACTION;
