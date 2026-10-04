# HTTPS local

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
