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

Con JWT de administrador o vendedor, usando la misma empresa utilizada para la vista previa:

```http
POST /api/comprobantes/ventas/123/whatsapp?idEmpresa=1
Authorization: Bearer TOKEN_JWT_DEL_SISTEMA
```

No tiene cuerpo ni parámetro para cambiar el destinatario: se lee de venta.cliente.telefono. Ocho dígitos reciben el prefijo 502; los internacionales conservan su código. Se permiten espacios, paréntesis, guiones y un signo + inicial. Un teléfono faltante o inválido requiere corregir los datos del cliente. Tener un formato válido no prueba que el número tenga WhatsApp.

El administrador/vendedor debe verificar el consentimiento del cliente antes de invocar el envío. En modo de prueba Meta solo permite los destinatarios autorizados correspondientes. Esta implementación usa una plantilla aprobada y no depende de que el cliente haya escrito en las últimas 24 horas.

Cada página PNG se sube por multipart a /PHONE_NUMBER_ID/media; después se envía por /PHONE_NUMBER_ID/messages como encabezado de la plantilla. La siguiente página empieza tras obtener el message ID anterior. El orden de solicitudes es secuencial; la entrega efectiva se confirma con webhooks (pendientes).

## Respuesta

```json
{
  "idVenta": 123,
  "estado": "aceptado",
  "totalPaginas": 1,
  "paginas": [
    { "pagina": 1, "estado": "aceptado", "messageId": "wamid...", "etapa": null, "codigoMeta": null }
  ]
}
```

HTTP 200 devuelve el resultado del intento; revisar siempre estado y paginas. Estados globales: aceptado, parcial, fallido, desconocido. Por página: aceptado, rechazado, fallido, desconocido, no_intentado. Etapas de fallo: generacion, subida, envio. Los errores de configuración o teléfono se devuelven como HTTP 400, y las entidades ausentes como 404.

Aceptado significa que Meta devolvió un identificador, no que el cliente recibió o leyó el mensaje. Si una solicitud de envío pierde la respuesta, devuelve estado desconocido; un error de subida no envía una plantilla. El servicio detiene las páginas posteriores e incluye las ya aceptadas, sin reintentos automáticos. El cuerpo privado del error de Meta no se devuelve ni se registra; solo su código numérico.

Importante: este servicio no persiste el historial de envíos ni implementa idempotencia entre solicitudes. Repetir el POST puede duplicar mensajes; no reintentar automáticamente una respuesta parcial/desconocida ni ejecutar dos envíos simultáneos de la misma venta. Si el solicitante se desconecta puede no recibir el resultado. Para producción falta acordar la persistencia e idempotencia, además del webhook; no se deben interpretar los resultados en memoria como un registro durable.

Cada página es un mensaje de plantilla separado, sujeto al tratamiento y cargos de Meta para esa plantilla. No envía un mensaje libre adicional.

## Pruebas

```powershell
dotnet run --project tests/WhatsApp.Checks
```

Utilizan un HttpMessageHandler simulado y credenciales ficticias; no leen secretos ni envían mensajes reales. Cubren teléfonos, payload y PNG, envío secuencial, fallo parcial, errores de subida, autorización rechazada, timeout, desconexión, respuesta inválida, configuración incompleta y cancelación.

Referencias oficiales:

* https://www.postman.com/meta/whatsapp-business-platform/request/2qbotrb/send-message-template-media
* https://whatsappbusiness.com/blog/media-messages-via-app/
