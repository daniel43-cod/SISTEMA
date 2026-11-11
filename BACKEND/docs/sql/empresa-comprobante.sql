-- Ejecutar una vez antes de utilizar los endpoints de comprobantes.
IF COL_LENGTH('dbo.empresa', 'ubicacion') IS NULL
    ALTER TABLE dbo.empresa ADD ubicacion nvarchar(150) NULL;
IF COL_LENGTH('dbo.empresa', 'direccion') IS NULL
    ALTER TABLE dbo.empresa ADD direccion nvarchar(250) NULL;
IF COL_LENGTH('dbo.empresa', 'telefonos') IS NULL
    ALTER TABLE dbo.empresa ADD telefonos nvarchar(150) NULL;
