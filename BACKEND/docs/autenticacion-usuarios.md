# Autenticación de usuarios internos

## Contrato para el frontend

- `POST /api/Login/Login`: recibe `usuario` y `password`. Devuelve los mismos campos de sesión que antes. Credenciales inválidas o cuenta/rol inactivos: `401` con mensaje genérico.
- `POST /api/Usuario` y `POST /api/Login/crear`: requieren `ADMINISTRADOR` y utilizan el mismo servicio y DTO. El segundo se conserva como alias.
- Ambas altas reciben `nombre`, `apellido`, `usuario`, `telefono`, `corre_electronico` (opcional), `password` e `id_rol`. La ruta `/api/Usuario` anteriormente aceptaba `correo`; ahora debe enviarse `corre_electronico`. No se aceptan como valores de negocio el estado, fecha, ID de usuario o relaciones enviados por el cliente.
- `GET /api/Usuario` y la respuesta del alta en `/api/Usuario` no incluyen contraseña ni hash. El alias sigue devolviendo el mensaje de confirmación.
- Validaciones: `400`; fallo inesperado: `500` con mensaje genérico y `traceId`. El detalle queda en los registros del servidor.
- Login: máximo 10 solicitudes por IP por ventana de un minuto, incluyendo intentos exitosos e inválidos; al excederlo devuelve `429` y `Retry-After`. El frontend debe respetar esa espera.

## Sesiones

Los JWT internos duran como máximo 30 minutos (o menos si `Jwt:DurationInMinutes` es menor). No se implementó renovación automática: al expirar se debe iniciar sesión de nuevo.

Cada petición con un JWT interno comprueba el usuario y su rol en la base de datos. Se deniega el acceso si no existen, están inactivos, el rol cambió o cambió el hash de contraseña. La versión de credencial es un HMAC; no incluye el hash de contraseña como dato visible del token. Si la base falla, se deniega la sesión.

Los tokens internos anteriores a este cambio no contienen la versión de credencial: los usuarios deben iniciar sesión nuevamente. No hay una lista permanente de revocación ni un endpoint de cierre de sesión en este cambio. Reactivar una cuenta sin cambiar contraseña/rol puede volver a permitir su token aún vigente.

Las cuentas internas se restringen a roles activos `ADMINISTRADOR` y `VENDEDOR`. Se conserva el flujo de JWT de clientes, sin consultar sus IDs en la tabla de usuarios internos.

## Integridad de las altas

Ambas rutas validan rol, contraseña y duplicados de usuario, correo y teléfono. Comprobación y escritura comparten una transacción `Serializable` para impedir altas simultáneas duplicadas a través de estos servicios. Los hashes se calculan antes de tomar bloqueos. Un conflicto de base de datos puede devolver el error genérico y requerir reintentar.

No se modificó el esquema ni se corrigieron registros existentes. El login rechaza nombres de usuario duplicados existentes. Es recomendable auditar los datos y agregar índices únicos en SQL Server para proteger también escrituras externas a la API. La comparación de identidad sigue la intercalación de la base de datos.

## Despliegue

- `appsettings.json` ya está excluido de Git. En producción configurar `Jwt__Key` mediante variables de entorno o un almacén de secretos; utilizar una clave aleatoria de al menos 32 bytes. La aplicación valida longitud, emisor, audiencia y duración al arrancar. La clave local existente no se rotó.
- Fuera de Development se activan HSTS y redirección HTTPS. Configurar certificado/puerto HTTPS. Si TLS termina en un proxy, configurar encabezados reenviados únicamente para proxies de confianza, incluyendo esquema e IP real; no confiar en encabezados arbitrarios del cliente.
- El limitador es local a cada instancia y reinicia su contador al reiniciar la aplicación. Para múltiples réplicas o ataques distribuidos se necesita un límite compartido en el gateway o almacenamiento distribuido. Usuarios detrás de la misma IP comparten el límite.
- El frontend debe manejar `401`, `403`, `400`, `429` y `500`. No debe registrar credenciales ni tokens en logs.

## Verificación

```powershell
dotnet build API_SISTEMA/API_SISTEMA.csproj
dotnet run --project tests/Auth.Checks/Auth.Checks.csproj
```

Las comprobaciones levantan HTTP local con controladores, JWT, filtro de errores y limitador reales; usan SQLite en memoria y datos ficticios. No leen credenciales de appsettings ni conectan a SQL Server. Cubren altas por ambas rutas, permisos, duplicados, respuestas, validación de sesiones y errores. No verifican concurrencia, intercalación ni índices contra SQL Server de producción.

Referencia del limitador: [documentación oficial ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit). Transacciones: [documentación oficial EF Core](https://learn.microsoft.com/ef/core/saving/transactions).
