# Login interno

## Ejecutar

1. Iniciar el backend: `dotnet run --project API_SISTEMA/API_SISTEMA.csproj --launch-profile http` desde BACKEND.
2. Desde Front-End ejecutar `npm run dev`.
3. Usar una cuenta interna existente, activa y con rol activo ADMINISTRADOR o VENDEDOR.

La petición POST /api/Login/Login envía `usuario` y `password`. Vite redirige /api a http://localhost:5272. Si cambia el puerto, copiar .env.example a .env.local, ajustar API_PROXY_TARGET y reiniciar Vite. No se abre CORS ni se cambian credenciales del backend.

En producción, servir la aplicación mediante HTTPS y configurar un reverse proxy para /api hacia ASP.NET. El proxy de Vite es una herramienta de desarrollo; publicar dist por sí solo no conecta la API.

## Organización

- app/providers: composición del proveedor de sesión.
- app/layouts: bienvenida tras autenticar y cierre de sesión.
- features/auth/api: endpoint y adaptación del contrato.
- features/auth/types: contratos TypeScript.
- features/auth/schemas: validaciones en ejecución de campos y respuesta.
- features/auth/session: sesión en memoria y vencimiento.
- features/auth/hooks: coordinación del formulario.
- features/auth/components: campos y presentación de errores.
- shared/api: HTTP, timeout, errores seguros; no depende de autenticación.
- shared/config: prefijo /api del mismo origen.

## Comportamiento

Token solo en memoria: recargar la página, cerrar la pestaña o cerrar sesión elimina la sesión local. El cierre local no revoca un JWT en el servidor. No hay renovación automática. La UI lee exp para cerrar al vencer y comprueba también al recuperar foco. Leer el JWT no verifica su firma ni concede permisos: esas comprobaciones pertenecen a la API.

No se guardan credenciales ni tokens en localStorage, sessionStorage, URLs o logs. Las peticiones usan no-store y la PWA no guarda respuestas API. Los permisos mostrados en la bienvenida son informativos; cada endpoint protegido debe seguir verificando autorización.

Errores contemplados: campos vacíos, límite UTF-8 de contraseña, 400, 401, 403, 429 respetando Retry-After, 5xx, conexión, timeout y respuesta inválida. No se reintenta automáticamente el login. El formulario cancela su solicitud al desmontarse.

La bienvenida confirma la sesión; los módulos de ventas y administración aún no están implementados.

## Comprobaciones

`npm run build`, `npm run lint` y `node --experimental-strip-types --test tests/auth.test.ts` (Node compatible con TypeScript nativo).

Las pruebas de contrato usan fetch simulado: no crean usuarios ni prueban contraseñas en la base real. Para verificar con SQL Server, ejecutar ambos servicios e iniciar sesión con una cuenta autorizada.

Referencia: [proxy de Vite](https://vite.dev/config/server-options).