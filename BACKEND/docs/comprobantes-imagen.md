# Comprobantes de venta como PNG

## Preparación

1. Aplicar `docs/sql/empresa-comprobante.sql` en SQL Server antes de usar Empresa o Comprobantes. No se ejecuta automáticamente.
2. Tener una empresa registrada (consultar `GET /api/Empresa/listar`, administrador).
3. Actualizar su encabezado con JWT de administrador:

```http
PUT /api/comprobantes/empresa/1
Content-Type: application/json

{
  "nombre": "Distribuidora San Antonio",
  "ubicacion": "Purulha B.V",
  "direccion": "Barrio San Antonio",
  "telefonos": null
}
```

Los valores anteriores son un ejemplo de configuración; el generador no los fija en código. Los campos vacíos se omiten. Este endpoint edita el nombre de la empresa existente y no modifica el NIT. La pantalla de edición en frontend no está incluida.

## Consultar imágenes

Administrador y vendedor, con token Bearer:

* `GET /api/comprobantes/ventas/123/paginas?idEmpresa=1`: cantidad de páginas.
* `GET /api/comprobantes/ventas/123/imagen?idEmpresa=1&pagina=1`: imagen PNG de esa página.

Debe seleccionarse explícitamente la empresa. El endpoint devuelve bytes `image/png`, no base64 ni archivos públicos. En el frontend, solicitar con autorización y visualizar mediante un Blob URL. Las respuestas tienen NoStore y limitación de solicitudes de ventas.

Ancho: 1200 píxeles. Alto variable hasta 2400. Repite encabezado y columnas, ajusta nombres y presentaciones a varias líneas y muestra totales solo en la última página. Descuento de línea: monto guardado, no porcentaje. Precio: unitario de la presentación. No se recalculan los importes históricos.

Se usan los nombres actuales del cliente, producto, presentación y empresa, y los importes y saldos actualmente guardados en la venta. No es una copia inmutable del comprobante emitido originalmente ni genera una factura fiscal.

La instalación debe disponer de Arial, DejaVu Sans o Liberation Sans. ImageSharp y Drawing requieren la licencia ya configurada en el entorno de compilación.

## Verificación

```powershell
dotnet build API_SISTEMA/API_SISTEMA.csproj --no-restore
dotnet run --project tests/Comprobantes.Checks -- artifacts/comprobantes
```

Las pruebas generan ejemplos sintéticos de 0, 2 y 100 detalles y verifican que no se pierdan filas, dimensiones máximas y archivos menores de 5 MB. No requieren conexión a SQL Server.

No incluye envío a Meta, webhooks, botón de frontend ni comprobantes de abonos/productos. El generador puede reutilizarse desde el futuro servicio de envío sin exponer el archivo por una URL pública.
