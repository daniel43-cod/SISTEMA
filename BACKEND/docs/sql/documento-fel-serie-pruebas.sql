-- Compatibilidad con la serie **PRUEBAS** que devuelve INFILE en Sandbox.
-- Ejecutar en instalaciones donde documento_fel ya fue creada con serie varchar(8).
IF OBJECT_ID('dbo.documento_fel', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.documento_fel', 'serie') < 20
    ALTER TABLE dbo.documento_fel ALTER COLUMN serie varchar(20) NULL;
