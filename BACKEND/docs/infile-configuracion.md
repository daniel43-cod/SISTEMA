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
  "tipo_identificacion_receptor": "NIT",
  "identificacion_receptor": "NIT_DEL_COMPRADOR",
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
El tipo y documento se eligen al certificar. Para NIT/CUI el backend consulta INFILE y guarda
el nombre registrado junto al documento en la venta, en la misma transacción que prepara el XML.
Para Q2,500.00 o más (total final con IVA) no se permite CF. El CUI debe tener 13 dígitos;
el XML incluye `TipoEspecial="CUI"`.
Se comprueban formato, límite de CF y consulta del NIT/CUI en INFILE.
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

### Consulta de nombre fiscal

`POST /api/Fel/receptor/consultar` requiere JWT del sistema, rol Administrador o Vendedor
y permiso Vender. Cuerpo: `{ "tipo": "NIT", "identificacion": "1170142K" }` o tipo CUI
con sus 13 dígitos. CF no consume estos servicios.
Devuelve resultado, tipo, identificacion, nombre, estatus (cuando existe) y descripcion.
Si no hay coincidencia o el CUI no está ACTIVO, el nombre no se completa.
Una respuesta no encontrada en INFILE no demuestra por sí sola que el NIT no exista en SAT.

Usa UsuarioApi/LlaveApi existentes, sin nuevas columnas ni credenciales en el frontend.
El NIT se consulta con JSON. Para CUI el backend autentica mediante multipart/form-data,
reutiliza el JWT por 110 minutos y lo renueva una vez si INFILE indica expiración/invalidez.
Las solicitudes concurrentes comparten la autenticación. No se devuelven tokens al navegador.

Al pulsar Certificar después de registrar una venta: elegir NIT/CUI, escribir documento y pulsar
Consultar (o Enter en ese campo). El nombre registrado aparece de solo lectura.
Cambiar documento o tipo invalida el nombre consultado. Certificar NIT/CUI exige una consulta
exitosa y el backend vuelve a verificar el documento; no confía en un nombre enviado por el navegador.
La misma verificación aplica si la petición omite tipo y documento y utiliza los guardados en la venta.
El nombre guardado manualmente no se usa como fuente para una nueva certificación NIT/CUI.
Después de la consulta, se comprueba nuevamente la identidad efectiva bajo bloqueo SQL; si cambió
durante la consulta se rechaza la operación, sin preparar ni enviar otro documento.
Recuperar o reconciliar un XML previamente enviado no vuelve a consultar ni modifica al receptor.
Los datos personales del cliente no se sobrescriben al consultar. La consulta no obtiene dirección.
Las pruebas automatizadas usan INFILE simulado; probar consultas reales con las credenciales habilitadas.
Se verificaron además estos formatos reales del servicio: NIT devuelve `{nit,nombre,mensaje}` sin
bandera resultado; CUI devuelve `{resultado,cui:{cui,nombre,fallecido}}`.
Se comprueba que el documento retornado coincida con el consultado y que el indicador fallecido
sea negativo. También se admiten los formatos planos de la documentación inicial.

Ejecutar `docs/sql/ventas-receptor-fiscal.sql` antes de iniciar esta versión del backend.
Las ventas históricas conservan las tres columnas en NULL, sin asumir que eran consumidor final.
Registrar o modificar ventas ya no requiere identificación fiscal. El frontend no incluye ese selector.
Para certificar una venta nueva como CF se envía:

```json
{
  "tipo_identificacion_receptor": "CF",
  "identificacion_receptor": "CF",
  "confirmar_bienes_gravados": true
}
```

Para NIT/CUI enviar el tipo y documento; la dirección es opcional.
El nombre se obtiene de INFILE. Estos datos quedan separados del nombre interno del cliente.
El panel obtiene el total desde `GET /api/Venta/{idVenta}/fel/preparacion` y bloquea CF
desde Q2,500.00, incluso si la venta se registró sin datos fiscales.
Por compatibilidad, las API de registro aceptan receptor explícito; si se omite, una venta nueva
queda con campos fiscales NULL y editar no borra los datos fiscales que ya existían.

`PUT /api/Venta/{idVenta}/receptor` permite completar estos campos sin modificar productos,
pagos ni inventario. Requiere rol Administrador/Vendedor y permiso Vender. Bloquea la edición
si existe un documento FEL certificado, en proceso o por confirmar; permite documentos rechazados,
cuyos XML anteriores se conservan. Sirve para completar las ventas históricas antes de certificar.
Para CF se omite por completo DireccionReceptor del XML y el frontend no solicita dirección,
código postal, municipio, departamento ni país. El cuerpo puede contener solamente
tipo CF, identificación CF y `confirmar_bienes_gravados: true`. Para NIT/CUI la dirección es opcional:
si sus cuatro campos de ubicación están vacíos, se omite DireccionReceptor; si se proporciona alguno,
se exige el bloque completo para evitar XML incompleto. No se utiliza la dirección del emisor
como dirección del comprador. La aceptación por INFILE sin dirección se debe comprobar en Sandbox.
Los reintentos POR_CONFIRMAR conservan el XML ya enviado, sin regenerarlo para quitar la dirección.
Un intento corresponde a un identificador y XML; la reconciliación del mismo identificador actualiza
sus mensajes. Una corrección tras rechazo crea otro intento. La recuperación manual de EN_PROCESO
queda disponible al vencer el plazo de procesamiento; requiere `documento-fel-recuperacion.sql`.

La certificación nueva y la corrección de un rechazo verifican siempre el receptor efectivo con
INFILE, aunque el cuerpo omita el tipo y se utilice el NIT/CUI guardado en la venta. El nombre
manual guardado no sustituye esta consulta. Solo se acepta el nombre oficial de la identificación
consultada y un CUI activo. Si la consulta falla, no se inicia un envío de certificación.
La comprobación del receptor se repite bajo el bloqueo de la venta para detectar cambios
concurrentes de identificación o un nuevo total que ya no permita CF. La reconciliación conserva
el receptor y el XML originales; no consulta otro receptor ni genera un documento diferente.

Pruebas locales: `dotnet run --project tests/Fel.Checks/Fel.Checks.csproj` usa un proveedor HTTP
simulado. Incluye receptores guardados y explícitos, límites CF, nombres vacíos/incorrectos,
documentos inválidos, CUI inactivo, respuestas para otra identificación, cancelación y errores
de autenticación. Las reglas de recuperación comprueban vencimiento y propiedad del intento;
no sustituyen una prueba de bloqueos y caídas del proceso contra SQL Server aislado.

Antes de marcar CERTIFICADA, `ValidarCertificadoFel` compara el bloque DatosEmision del XML
original almacenado con el certificado recibido: emisor, receptor, fecha, tipo, moneda, frases,
líneas, precios, descuentos, impuestos y totales. Compara números exactamente, aceptando ceros
decimales equivalentes, e ignora prefijos XML, declaraciones de espacios de nombres, comentarios,
indentación y orden de atributos. La estructura y el orden de las líneas deben coincidir.
UUID, serie y número deben corresponder a NumeroAutorizacion y se requiere fecha de certificación.
Se prohíben DTD y resolución externa, y el XML recibido se limita a 10 MB antes de procesarlo.

Si la respuesta declara éxito pero falla esta comprobación, el estado sigue POR_CONFIRMAR.
El intento conserva el XML recibido cuando se puede decodificar de forma segura, los metadatos
normalizados de INFILE y una descripción del fallo local. No modifica los datos originales ni
genera un nuevo identificador. Una recuperación sin respuesta conserva ese XML para revisión.
No requiere cambios de esquema. Esta comparación no constituye verificación criptográfica de
las firmas. `CertificadoFelChecks` cubre alteraciones, campos faltantes, duplicados y formatos
equivalentes con respuestas simuladas; la prueba integral contra SQL Server sigue pendiente.
