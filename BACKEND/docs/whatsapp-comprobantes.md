# Envío de comprobantes por WhatsApp

## Configuración local

Agregar o combinar esta sección con los secretos existentes del proyecto (no guardar el token en Git):

```json
{
  "WhatsApp": {
    "AccessToken": "TOKEN_PRIVADO",
    "PhoneNumberId": "ID_DEL_NUMERO_REMITENTE",
    "BusinessAccountId": "ID_DE_LA_CUENTA",
    "ApiVersion": "v27.0",
    "TemplateName": "envio_de_comprobante",
    "TemplateLanguage": "es_PA"
  }
}
```

ASP.NET Core carga User Secrets automáticamente en Development. Un archivo creado en otra carpeta de AppData no se carga automáticamente: debe estar en la ubicación correspondiente al UserSecretsId del proyecto. No exponer el contenido del archivo. El WABA ID se conserva para administración de cuenta; las rutas de subida y envío usan PhoneNumberId.

La plantilla debe estar aprobada en esa cuenta y ese idioma, con encabezado IMAGE y sin variables de cuerpo ni botones. Si se modifica su estructura, debe adaptarse el payload. ApiVersion, TemplateName y TemplateLanguage son configurables.

## Solicitud

Antes de utilizar estas rutas, aplicar `docs/postgresql.md` y `docs/postgresql.md` en PostgreSQL. Si ya existían las tablas de la versión con recuperación selectiva, detener el backend anterior y aplicar `docs/postgresql.md` antes de iniciar esta versión. Quita la restricción única por venta/empresa y elimina snapshot_json con sus copias antiguas; conserva los registros de envío, páginas, intentos y eventos. No se ejecuta automáticamente. Cada acción Compartir envía el comprobante completo de nuevo, aun cuando no haya cambios.

Con JWT de administrador o vendedor, usando la misma empresa utilizada para la vista previa:

```http
POST /api/comprobantes/ventas/123/whatsapp?idEmpresa=1
Authorization: Bearer TOKEN_JWT_DEL_SISTEMA
```

La ruta habitual no tiene cuerpo ni parámetro para cambiar el destinatario: usa cliente.telefono. Ocho dígitos reciben el prefijo 502; los internacionales conservan su código. Se permiten espacios, paréntesis, guiones y un signo + inicial. Tener un formato válido no prueba que el número tenga WhatsApp.

## Botones del frontend y teléfono faltante

No se requiere agregar una columna: se reutiliza cliente.telefono. Si se ejecutó el script anterior que agregaba ventas.telefono_comprobante, aplicar `docs/postgresql.md` para trasladar los datos compatibles al cliente y retirar esa columna. El script no sobrescribe teléfonos existentes y se detiene ante conflictos, sin elegir un número arbitrariamente. Comparaciones con distinto formato también requieren revisión. Si nunca se creó la columna, el script no hace cambios.

Al registrar una venta (o confirmar una venta de pedido) se abre un cuadro opcional con el botón e icono de WhatsApp. Registrar por sí solo nunca envía. Ahora no/Cerrar tampoco envía. El historial tiene botones por venta tanto en escritorio como en móvil.

El cuadro consulta `GET /api/comprobantes/ventas/{id}/opciones-whatsapp` para obtener el teléfono actual y las empresas disponibles. No se fija idEmpresa en el frontend. Si hay una sola se utiliza esa; si hay varias se solicita seleccionarla.

* Si ya hay teléfono, se muestra como destino y no se permite reemplazarlo al enviar.
* Al terminar el registro y faltar teléfono, se solicita uno. Al pulsar Enviar se llama a `POST /api/comprobantes/ventas/{id}/whatsapp/registro?idEmpresa=...` con `{ "telefono": "55551234" }`. El backend lo guarda normalizado en cliente.telefono antes de enviar. Si el envío falla, ese teléfono queda guardado. Las demás ventas del mismo cliente también lo usarán al compartir.
* Desde el historial y sin teléfono registrado, el mismo cuerpo se envía a `POST /api/comprobantes/ventas/{id}/whatsapp/historial?idEmpresa=...`. Solo se usa como destinatario de esa solicitud, sin modificar venta ni cliente. Sigue figurando como destinatario en el registro de WhatsApp.

Un campo vacío muestra "Ingresa el número de teléfono primero." Ambas rutas rechazan un teléfono alternativo si entretanto ya existe uno registrado. La asignación al registrar verifica esa condición de nuevo en SQL para evitar sobrescrituras concurrentes.

El botón se deshabilita durante su solicitud; no hay reintentos automáticos. El cuadro muestra el mensaje del backend y consulta la entrega mientras permanece abierto (hasta 20 consultas cada 3 segundos). Al cerrar se detiene la consulta; no se implementan notificaciones push. Si no llega una confirmación, no se presume fallo. El POST de comprobante admite hasta tres minutos en el cliente, manteniendo quince segundos por defecto para las demás solicitudes.

El perfil webhook-local también escucha en http://localhost:5272 para el proxy de desarrollo existente del frontend, además de https://localhost:7272 y del puerto 5273 restringido al webhook. Ejecutar una sola instancia del backend.

El administrador/vendedor debe verificar el consentimiento del cliente antes de invocar el envío. En modo de prueba Meta solo permite los destinatarios autorizados correspondientes. Esta implementación usa una plantilla aprobada y no depende de que el cliente haya escrito en las últimas 24 horas.

Cada página PNG se sube por multipart a /PHONE_NUMBER_ID/media; después se envía por /PHONE_NUMBER_ID/messages como encabezado de la plantilla. La siguiente página empieza tras obtener el message ID anterior. El orden de solicitudes es secuencial; la entrega efectiva se confirma con el [webhook](whatsapp-webhook-desarrollo.md), que requiere aplicar su script y configurar la suscripción de Meta.

## Respuesta

```json
{
  "idEnvio": "7ba3e9d8-df79-47cb-86b8-6f41359225ec",
  "resultado": {
    "idVenta": 123,
    "estado": "aceptado",
    "totalPaginas": 1,
    "paginas": [
      { "pagina": 1, "estado": "aceptado", "messageId": "wamid...", "etapa": null, "codigoMeta": null }
    ]
  },
  "huboFallo": false,
  "mensaje": "El comprobante fue aceptado por WhatsApp. La entrega está pendiente de confirmación."
}
```

HTTP 200 devuelve el resultado del intento; revisar siempre huboFallo, mensaje, resultado.estado y resultado.paginas. Estados globales: pendiente, en_proceso, aceptado, parcial, fallido, desconocido, fallido_entrega, entregado y leido. Por página: no_intentado, subiendo, enviando, aceptado, rechazado, fallido, desconocido. Etapas de fallo: generacion, subida, envio. Los errores de configuración o teléfono se devuelven como HTTP 400 y las entidades ausentes como 404.

Aceptado significa que Meta devolvió un identificador, no que el cliente recibió o leyó el mensaje. Si una solicitud de envío pierde la respuesta, devuelve estado desconocido; un error de subida no envía una plantilla. El servicio detiene las páginas posteriores e incluye las ya aceptadas, sin reintentos automáticos. El cuerpo privado del error de Meta no se devuelve ni se registra; solo su código numérico.

## Estado e historial de acciones Compartir

```http
GET /api/comprobantes/whatsapp/7ba3e9d8-df79-47cb-86b8-6f41359225ec
GET /api/comprobantes/ventas/123/whatsapp/historial?idEmpresa=1&pagina=1&tamanoPagina=20
```

Ambas rutas requieren JWT de administrador/vendedor. El historial devuelve pagina, tamanoPagina, totalEnvios y envios, ordenados del más reciente al más antiguo. Cada envío contiene resumen, mensaje, huboFallo e intentos con usuario, hora UTC, estado, código de error y message ID. El tamaño permitido es de 1 a 50. No expone credenciales.

Cada POST registra un envío nuevo, con el teléfono normalizado actual del cliente, remitente y plantilla configurados. Genera todas las páginas con la venta y empresa consultadas en esa solicitud. No guarda PNG ni copia JSON de los datos. El script de actualización elimina la columna snapshot_json y sus copias antiguas; el modelo nuevo no la consulta ni escribe.

Compartir nuevamente siempre inicia un envío completo con otro idEnvio, aunque el anterior se haya entregado o no haya cambiado la venta. Si hubo un fallo parcial, también vuelve a mandar las páginas que sí habían sido aceptadas. No se recuperan páginas antiguas ni se exige resolver antes un resultado desconocido. Se retiró POST /api/comprobantes/whatsapp/{idEnvio}/reenviar. No hay reintentos automáticos del frontend o backend: cada POST debe corresponder a una acción del usuario.

Dos acciones explícitas pueden ejecutarse a la vez; no se bloquean entre sí. Dentro de cada acción, las páginas se solicitan en orden y se detienen ante el primer fallo. Los estados se guardan antes de contactar a Meta, sin mantener transacciones SQL durante HTTP. Un registro que lleva más de diez minutos en subida/envío se muestra como desconocido, sin reintentar por su cuenta.

Tras configurar el webhook, el historial incluye estadoEntrega y codigoEntrega. Si una página falla durante el envío o Meta confirma después un fallo de entrega, huboFallo es true y mensaje informa: "Hubo un fallo al compartir el comprobante. Intenta nuevamente más tarde." El cuadro de envío del frontend muestra ese mensaje y consulta el estado por idEnvio mientras está abierto. No incluye notificaciones push. La falta de confirmación de entrega por sí sola no prueba que haya fallado.

Las tablas existentes se conservan para historial y estados. No es necesario eliminarlas: envio registra cada acción, pagina sus imágenes, intento el resultado por página y webhook_evento las notificaciones de Meta. No contienen tokens. El envío no modifica importes ni pagos; únicamente el flujo de registro puede guardar el teléfono faltante en el cliente.

Cada página es un mensaje de plantilla separado, sujeto al tratamiento y cargos de Meta para esa plantilla. No envía un mensaje libre adicional.

## Pruebas

```powershell
dotnet run --project tests/WhatsApp.Checks
dotnet run --project tests/WhatsAppHistorial.Checks
```

Utilizan un HttpMessageHandler simulado y credenciales ficticias; no leen secretos ni envían mensajes reales. Cubren teléfonos, payload y PNG, envío secuencial, fallo parcial, errores de subida, autorización rechazada, timeout, desconexión, respuesta inválida, configuración incompleta y cancelación.

Las pruebas de historial usan SQLite temporal y cubren múltiples acciones Compartir de la misma venta, envío completo tras fallo parcial, imagen nueva al modificar la venta, solicitudes simultáneas, avisos de fallo, resultados desconocidos sin bloqueo de nuevas acciones y paginación del historial. Verifican la lógica relacional sin conectarse al PostgreSQL real; el script PostgreSQL requiere aplicación en el entorno del negocio.

Referencias oficiales:

* https://www.postman.com/meta/whatsapp-business-platform/request/2qbotrb/send-message-template-media
* https://whatsappbusiness.com/blog/media-messages-via-app/
