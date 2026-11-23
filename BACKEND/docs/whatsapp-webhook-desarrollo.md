# Webhook de WhatsApp con Cloudflare Quick Tunnel

## Preparación

Cloudflared 2026.11.0 está instalado mediante el paquete Cloudflare.cloudflared de winget. Si la terminal no encuentra `cloudflared`, abrir una nueva. El script también busca su ejecutable en Program Files. No se instala como servicio ni se inicia al encender Windows.

1. Aplicar `docs/postgresql.md` después de `whatsapp-historial.sql`. El nuevo script crea una tabla de estados de entrega, sin modificar las ventas ni enviar mensajes. No se ejecuta automáticamente.
2. En Meta for Developers, abrir la aplicación > Configuración de la aplicación > Básica > Secreto de la aplicación > Mostrar. No es el AccessToken ni el WABA ID.
3. Agregar estas propiedades a la sección WhatsApp de los secretos del backend, conservando las demás:

```json
{
  "WhatsApp": {
    "AppSecret": "SECRETO_DE_LA_APLICACION_DE_META",
    "VerifyToken": "CADENA_PRIVADA_ALEATORIA_ELEGIDA_POR_TI"
  }
}
```

Elegir una cadena larga y aleatoria para VerifyToken y poner exactamente el mismo valor en Meta. AppSecret valida la firma de los POST; VerifyToken valida la suscripción GET. No compartirlos ni incluirlos en Git, capturas o comandos pegados públicamente. AccessToken, PhoneNumberId y BusinessAccountId deben seguir configurados.

## Iniciar backend y túnel

Desde BACKEND, primera terminal:

```powershell
dotnet run --project API_SISTEMA --launch-profile webhook-local
```

Este perfil inicia Swagger/API en https://localhost:7272, mantiene http://localhost:5272 para el proxy local del frontend y agrega un puerto HTTP solo de loopback para el túnel en http://localhost:5273. Un middleware restringe el puerto 5273 exclusivamente a `/api/whatsapp/webhook`; no publica Swagger ni el resto de la API. Comprueba el puerto real de la conexión, no el encabezado Host. No usar otro perfil para ese puerto ni ejecutar otra instancia que ocupe el 5272.

Segunda terminal:

```powershell
.\scripts\IniciarTunelWhatsApp.ps1
```

El script verifica que Swagger no esté disponible y que el webhook tenga las credenciales necesarias antes de iniciar cloudflared. No salta políticas de ejecución de PowerShell ni desactiva validaciones TLS. Si Windows bloquea scripts, consultar la política antes de cambiarla; el comando equivalente, tras las comprobaciones, es `cloudflared tunnel --url http://localhost:5273`.

Cloudflared imprime una dirección https://...trycloudflare.com. La conexión pública está protegida por el certificado de Cloudflare; el último tramo HTTP es únicamente loopback en la misma computadora. No se requiere abrir puertos del router. No usar `--no-tls-verify`. La dirección cambia cada vez que se crea el túnel. Las dos terminales deben permanecer abiertas; Ctrl+C detiene cada proceso. No hay un túnel permanentemente activo por instalar la herramienta.

## Configurar Meta

En la aplicación > WhatsApp > Configuración (o Casos de uso > Personalizar > Configuración):

* Callback URL: `https://LA_URL_DEL_TUNEL.trycloudflare.com/api/whatsapp/webhook`
* Verify token: el valor privado que guardaste en WhatsApp:VerifyToken.

Verificar y guardar. Luego suscribirse al campo **messages** para recibir estados. Verificar también que la aplicación esté suscrita a la cuenta de WhatsApp correspondiente. No agregar autenticación por email al túnel, pues Meta no puede completar una sesión interactiva. Cada dirección nueva requiere actualizar Callback URL en Meta.

El GET correcto devuelve el challenge en texto plano. Un token incorrecto produce 403; configuración faltante, 503. Los POST deben incluir X-Hub-Signature-256 calculada con HMAC SHA256 sobre los bytes originales y el AppSecret. Firmas ausentes/incorrectas producen 401. Máximo de cuerpo: 2 MiB.

## Estados e historial

Se conservan eventos sent, delivered, read y failed solo si coinciden WABA ID y Phone Number ID configurados. No se guardan cuerpos de chats ni se responden mensajes entrantes. La huella de cada evento evita duplicados. Un 200 se devuelve después de guardar los eventos en SQL; si falla la persistencia no se confirma el lote.

En el historial, cada página mantiene su estado de solicitud y agrega estadoEntrega (enviado/entregado/leido/fallido) y codigoEntrega. Lectura y entrega no retroceden por eventos atrasados. Los eventos se correlacionan por message ID y remitente; si llegan antes de que se guarde la respuesta del envío, siguen disponibles para la consulta posterior.

Un fallo de entrega confirmado hace que la consulta del envío devuelva huboFallo=true y un mensaje para intentar más tarde. Si el usuario presiona Compartir otra vez, se genera el comprobante completo con los datos actuales y un idEnvio nuevo. No hay recuperación selectiva ni reintentos automáticos. Una entrega/lectura confirmada no impide compartir nuevamente. Los eventos anteriores solo afectan a su message ID. Si una respuesta perdida dejó un envío sin message ID, permanece desconocido, pero no bloquea otra acción Compartir. Consultar el estado por GET /api/comprobantes/whatsapp/{idEnvio} o listar todos los envíos de la venta mediante el historial paginado.

La tabla de eventos conserva identificadores y códigos, no credenciales. Aplicar los permisos y respaldos del resto de los datos comerciales. Este túnel es temporal de desarrollo; no sustituye el despliegue de producción.

## Pruebas

```powershell
dotnet run --project tests/WhatsAppWebhook.Checks
dotnet run --project tests/WhatsAppHistorial.Checks
```

SQLite temporal y HTTP simulado: no utilizan secretos reales ni envían mensajes. Incluyen challenge, firma válida/manipulada, rechazo antes de SQL, deduplicación de notificaciones, cuenta/remitente ajenos, eventos desordenados, avisos de fallo y nuevos envíos completos independientes después de entrega o fallo.

Referencias oficiales:

* https://developers.facebook.com/documentation/business-messaging/whatsapp/webhooks/create-webhook-endpoint
* https://developers.cloudflare.com/tunnel/get-started/quick-tunnels/
