# HTTPS local

## Solo esta computadora: certificado de desarrollo de .NET

Para Swagger o llamadas a la API desde el navegador de esta computadora:

```powershell
dotnet dev-certs https --check --trust
dotnet run --project API_SISTEMA --launch-profile https-local
```

Abrir https://localhost:7272/swagger. El perfil https-local usa el certificado del almacén del usuario; no requiere los PEM de .certs. En el equipo actual ya se verificó que es confiable. En otro equipo, si la comprobación falla, ejecutar `dotnet dev-certs https --trust` y seguir el aviso del sistema. La configuración es por equipo/usuario, no por cada inicio.

HTTPS cifra la conexión navegador/API y permite comprobar la identidad local del servidor. No sustituye JWT, permisos ni la protección del token de Meta. No modifica ventas ni credenciales.

HTTPS usa el puerto 7272; el perfil http conserva localhost:5272. `dotnet run --project API_SISTEMA` sigue usando el primer perfil (http): seleccionar explícitamente https-local para HTTPS. No iniciar dos instancias en el mismo puerto. launchSettings.json es para desarrollo local, no configura producción.

El certificado tiene clave privada y vencimiento: no compartir ni subir la clave a Git. Si caduca, renovar con .NET. Evitar `--clean` como primer paso, pues elimina certificados que pueden usar otros proyectos. No desactivar la validación TLS para ocultar errores. Referencia: https://learn.microsoft.com/dotnet/core/tools/dotnet-dev-certs

Este perfil cubre localhost. No configura celulares u otros equipos ni una IP LAN; para eso se conserva el procedimiento mkcert de abajo. Si el frontend llama directamente a la API, cambiar su URL base a https://localhost:7272 y revisar CORS. Si usa un proxy, actualizar su destino y comprobar la confianza en el certificado. Una página HTTPS no debe llamar a una API HTTP por contenido mixto. Este cambio no modifica el frontend.

Las solicitudes salientes a Meta ya usan HTTPS. Para el webhook, Meta no puede llamar a localhost: necesita una URL HTTPS pública. El certificado local no la proporciona. El perfil webhook-local y Cloudflare Quick Tunnel se describen en [whatsapp-webhook-desarrollo.md](whatsapp-webhook-desarrollo.md). HTTPS de producción es una configuración separada.

## Red local y Android: certificados mkcert

El perfil anterior `https` sigue usando los PEM y scripts siguientes; es distinto de `https-local`.

1. Instalar mkcert desde sus releases oficiales: https://github.com/FiloSottile/mkcert
2. Desde BACKEND ejecutar:
   .\scripts\PrepararHttps.ps1 -LanIp 192.168.0.3
   El script instala una autoridad local confiable y genera certificados en .certs (ignorada en Git).
3. Arrancar la API:
   dotnet run --project API_SISTEMA --launch-profile https
4. En otra terminal:
   .\scripts\IniciarFrontendHttps.ps1
5. Computadora: https://localhost:5173
   Android: https://192.168.0.3:5173
   Swagger: https://localhost:7272/swagger

En Android instalar únicamente .certs/rootCA.pem como certificado de CA confiable.
Nunca transferir dev-key.pem ni rootCA-key.pem. Las pantallas de instalación varían por dispositivo.
Permitir el puerto 5173 en firewall solo en red privada/local; API permanece en localhost.
Vite escucha en las interfaces del equipo para permitir localhost y LAN. No exponerlo a Internet.
Si la IP cambia, regenerar el certificado con la nueva IP.
Node usa NODE_EXTRA_CA_CERTS; el proxy mantiene secure: true.
Las rutas del certificado del perfil se resuelven desde la carpeta del proyecto API_SISTEMA.
No publicar estos certificados: producción requiere certificado público y proxy configurado.
HTTPS todavía no recupera sesiones; cookies HttpOnly y renovación son el paso siguiente.
