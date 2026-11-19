# Configuración privada de INFILE

El backend lee la sección `Infile` de la configuración de ASP.NET Core.
No guardar credenciales en SQL, archivos versionados ni en el frontend.
El proyecto ya dispone de UserSecretsId. En desarrollo se puede usar **Administrar secretos de usuario**
en Visual Studio y agregar esta sección al JSON privado, conservando las demás secciones existentes:

```json
{
  "Infile": {
    "Habilitado": true,
    "Ambiente": "PRUEBAS",
    "UsuarioApi": "REEMPLAZAR",
    "LlaveApi": "REEMPLAZAR",
    "UsuarioFirma": "REEMPLAZAR",
    "LlaveFirma": "REEMPLAZAR"
  }
}
```

Los valores REEMPLAZAR son marcadores, no credenciales válidas.
User Secrets se carga por defecto en el ambiente Development; es almacenamiento local sin cifrar.
No confundir ASPNETCORE_ENVIRONMENT con el estado de implementación del emisor en INFILE.

Para variables de entorno los nombres equivalentes son:

| Variable | Valor |
| --- | --- |
| `Infile__Habilitado` | `true` |
| `Infile__Ambiente` | `PRUEBAS` o `PRODUCCION` |
| `Infile__UsuarioApi` | Usuario API asignado |
| `Infile__LlaveApi` | Llave API asignada |
| `Infile__UsuarioFirma` | Usuario de firma asignado |
| `Infile__LlaveFirma` | Llave de firma asignada |

Las variables de entorno tienen prioridad sobre User Secrets. Reiniciar el backend después de cambiarlas.
Sin configuración, FEL queda deshabilitado. Al habilitarlo, se valida que existan las cuatro credenciales;
esta validación no comprueba su autenticidad con INFILE.

**Ambiente no cambia el endpoint ni convierte al emisor en Sandbox.** INFILE debe confirmar que
el emisor se encuentra en estado Implementación antes de enviar documentos de prueba.
El endpoint de pruebas puede realizar llamadas externas cuando se invoca explícitamente.

## Frases y endpoint de pruebas

Agregar en la sección privada `Infile` las frases autorizadas para el emisor de pruebas:

```json
"Frases": [ { "Tipo": 1, "Escenario": 1 }, { "Tipo": 2, "Escenario": 1 } ]
```

`POST /api/Venta/{idVenta}/certificar`, con JWT interno y permiso Vender, admite:

```json
{
  "direccion": "CIUDAD",
  "codigo_postal": "01001",
  "municipio": "GUATEMALA",
  "departamento": "GUATEMALA",
  "pais": "GT",
  "confirmar_bienes_gravados": true
}
```

Usar datos adecuados al caso. Este ejemplo no determina si corresponde consumidor final.
La versión inicial solo admite bienes gravados al 12%, GTQ y régimen GEN. Las líneas usan UND
para la presentación vendida. No admite exenciones ni servicios por este endpoint.
La identificación y el nombre se toman de la venta, no del cuerpo de certificación.
Para Q2,500.00 o más (total final con IVA) no se permite CF. El CUI debe tener 13 dígitos;
el XML incluye `TipoEspecial="CUI"`.
Esta validación comprueba formato y límite de CF; no consulta la existencia del NIT/CUI en INFILE.
La fecha de emisión es la fecha actual del envío inicial; no se copia la fecha histórica de venta.
Se valida localmente la estructura pre-firma contra una proyección del XSD SAT conservado en Schemas/Fel.
Solo se omiten de esa proyección los elementos de firma que agregará INFILE. El original no se modifica.
Producción permanece bloqueada hasta revisar reglas fiscales y completar pruebas integrales.

`GET /api/Venta/{idVenta}/fel` consulta el estado del ambiente configurado.
No se vuelve a registrar inventario ni pagos. Cualquier documento FEL bloquea editar la venta.
CERTIFICADA devuelve el resultado existente. POR_CONFIRMAR permite repetir POST conservando
el XML y el identificador originales (los cambios del cuerpo no se aplican).
RECHAZADA permite repetir POST con los datos corregidos: se genera otro intento y un nuevo identificador,
conservando los intentos previos. El emisor se vuelve a leer desde empresa.
Se requiere haber ejecutado documento-fel-normalizar-respuesta.sql. Si falta el intento histórico,
la corrección se bloquea para evitar perder el rechazo anterior.
EN_PROCESO no permite otro envío concurrente. Si el proceso se detiene después de guardar ese
estado, se necesita reconciliar la operación antes de reintentar; no borrar el registro ni cambiar su identificador.
Las respuestas nuevas se descomponen en documento_fel_intento, documento_fel_error y documento_fel_alerta;
no se escriben nuevos JSON en documento_fel.respuesta. Esa columna permanece como archivo histórico
de datos anteriores; no se elimina en esta etapa y ya no representa necesariamente el último resultado.
La respuesta de POST y GET incluye id_intento, descripcion, errores y alertas del último intento.

## Receptor guardado en la venta

Ejecutar `docs/sql/ventas-receptor-fiscal.sql` antes de iniciar esta versión del backend.
Las ventas históricas conservan las tres columnas en NULL, sin asumir que eran consumidor final.
Crear y modificar ventas requiere elegir explícitamente CF, NIT o CUI:

```json
{
  "tipo_identificacion_receptor": "CF",
  "identificacion_receptor": "CF",
  "nombre_receptor": "CONSUMIDOR FINAL"
}
```

Para NIT/CUI enviar el documento y el nombre fiscal. Los valores quedan separados del cliente,
para que cambios posteriores del cliente no cambien el receptor de la venta.
Al confirmar pedidos de catálogo se envían los mismos datos con nombres PascalCase:
`TipoIdentificacionReceptor`, `IdentificacionReceptor`, `NombreReceptor`.
El frontend debe incorporar el selector; sin selección el backend rechaza registrar la venta.

`PUT /api/Venta/{idVenta}/receptor` permite completar estos campos sin modificar productos,
pagos ni inventario. Requiere rol Administrador/Vendedor y permiso Vender. Bloquea la edición
si existe un documento FEL certificado, en proceso o por confirmar; permite documentos rechazados,
cuyos XML anteriores se conservan. Sirve para completar las ventas históricas antes de certificar.
La dirección y confirmación fiscal todavía se reciben en el cuerpo de certificación;
este cambio no elimina todo el cuerpo del endpoint ni inventa datos geográficos del cliente.
Un intento corresponde a un identificador y XML; la reconciliación del mismo identificador actualiza
sus mensajes. Una corrección tras rechazo crea otro intento. El flujo aún no recupera automáticamente
operaciones que quedaron EN_PROCESO al detenerse el servidor.
