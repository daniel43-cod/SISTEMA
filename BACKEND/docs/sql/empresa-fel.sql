-- Ejecutar después de empresa-comprobante.sql y antes de desplegar el modelo actualizado.
-- Campos opcionales para conservar las empresas existentes. Completar antes de certificar.
-- No almacenar usuarios, llaves API ni llaves de firma en esta tabla.
IF COL_LENGTH('dbo.empresa', 'razon_social') IS NULL
    ALTER TABLE dbo.empresa ADD razon_social nvarchar(250) NULL;
IF COL_LENGTH('dbo.empresa', 'nombre_comercial') IS NULL
    ALTER TABLE dbo.empresa ADD nombre_comercial nvarchar(250) NULL;
IF COL_LENGTH('dbo.empresa', 'afiliacion_iva') IS NULL
    ALTER TABLE dbo.empresa ADD afiliacion_iva nvarchar(10) NULL;
IF COL_LENGTH('dbo.empresa', 'codigo_establecimiento') IS NULL
    ALTER TABLE dbo.empresa ADD codigo_establecimiento int NULL;
IF COL_LENGTH('dbo.empresa', 'correo_emisor') IS NULL
    ALTER TABLE dbo.empresa ADD correo_emisor nvarchar(254) NULL;
IF COL_LENGTH('dbo.empresa', 'codigo_postal') IS NULL
    ALTER TABLE dbo.empresa ADD codigo_postal nvarchar(10) NULL;
IF COL_LENGTH('dbo.empresa', 'municipio') IS NULL
    ALTER TABLE dbo.empresa ADD municipio nvarchar(100) NULL;
IF COL_LENGTH('dbo.empresa', 'departamento') IS NULL
    ALTER TABLE dbo.empresa ADD departamento nvarchar(100) NULL;
IF COL_LENGTH('dbo.empresa', 'pais') IS NULL
    ALTER TABLE dbo.empresa ADD pais nvarchar(2) NULL;
