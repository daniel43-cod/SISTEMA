# Sistema — Distribuidora San Antonio

Frontend del sistema de punto de venta y gestión comercial. Incluye un panel para personal y un catálogo público con carrito, pedidos y asistente para clientes.

Desarrollado con React 19, TypeScript 6, Vite 8 y `vite-plugin-pwa`. Este directorio contiene el frontend: necesita el backend del sistema y su base de datos para autenticar usuarios y realizar operaciones.

## Funcionalidades

- Ventas, historial, pagos y caja compartida.
- Productos, categorías, marcas, presentaciones e inventario.
- Compras, gastos, pedidos, usuarios y reportes de ventas, caja e inventario.
- Catálogo público, autenticación de clientes, carrito y pedidos.
- Asistente de clientes mediante el backend e instalación como PWA.

El rol `VENDEDOR` accede a ventas y productos. `ADMINISTRADOR` dispone de los módulos adicionales. El backend debe validar los permisos de cada operación.

## Organización del código fuente

```text
Front-End/
├── src/
│   ├── app/             # Aplicación, proveedores, navegación, layouts y páginas generales
│   ├── features/        # Módulos funcionales
│   │   ├── auth/        # Autenticación del personal
│   │   ├── customer-auth/   # Autenticación de clientes
│   │   ├── customer-chat/   # Asistente
│   │   ├── public-catalog/  # Catálogo y carrito
│   │   ├── catalog-lookups/ # Consultas compartidas de catálogos
│   │   ├── products/    # Productos
│   │   ├── categories/  # Categorías
│   │   ├── brands/      # Marcas
│   │   ├── presentations/ # Presentaciones
│   │   ├── sales/       # Ventas y pagos
│   │   ├── cash/        # Caja
│   │   ├── inventory/   # Inventario
│   │   ├── purchases/   # Compras
│   │   ├── expenses/    # Gastos
│   │   ├── orders/      # Pedidos
│   │   ├── reports/     # Reportes
│   │   └── users/       # Usuarios internos
│   ├── shared/          # Cliente HTTP, configuración, caché y componentes comunes
│   ├── styles/          # Estilos globales
│   ├── assets/          # Recursos gráficos
│   └── main.tsx         # Montaje de React
├── public/              # Recursos públicos e icono PWA
├── tests/               # Pruebas con el runner de Node.js
├── docs/                # Documentación de login y ventas
├── .env.example         # Plantilla sin secretos
├── vite.config.ts       # Vite, proxy, HTTPS local y PWA
├── nginx.conf           # Servidor del contenedor
├── Dockerfile           # Compilación y servicio con Nginx
└── package.json         # Dependencias y comandos
```

Cada módulo agrupa páginas, componentes, acceso a API, tipos, validaciones y hooks según sus necesidades. El cliente HTTP común está en `src/shared/api/httpClient.ts`. La base de la API, definida en `src/shared/config/api.ts`, es `/api`.

## Requisitos e instalación

Necesitas Node.js 22.12 o superior (se recomienda Node.js 22 LTS actualizado), npm, un navegador moderno y el backend configurado con su base de datos y usuarios.

Clona el repositorio de GitHub y abre el directorio de este README. Desde la raíz del repositorio actual:

```sh
cd Front-End.web/Front-End
npm ci
```

Copia la plantilla de configuración. En PowerShell:

```powershell
Copy-Item .env.example .env.local
```

En Linux o macOS:

```sh
cp .env.example .env.local
```

Edita `.env.local` para indicar la dirección del backend. No necesitas instalar paquetes globalmente.

## Variables de entorno

Las siguientes variables las lee `vite.config.ts` en el proceso de Vite. No llevan prefijo `VITE_` ni se incorporan como variables de aplicación al navegador.

| Variable | Obligatoria | Descripción |
| --- | --- | --- |
| `API_PROXY_TARGET` | No | Destino del proxy de desarrollo para `/api` y `/uploads`. Ejemplo: `http://localhost:5272`. Si se omite, usa esa dirección con HTTP o `https://localhost:7272` cuando se activa HTTPS local. |
| `DEV_HTTPS_CERT` | No | Ruta al certificado PEM para HTTPS local, por ejemplo `C:/certificados/dev-cert.pem`. Debe configurarse junto con la clave. |
| `DEV_HTTPS_KEY` | No | Ruta a la clave privada PEM correspondiente, por ejemplo `C:/certificados/dev-key.pem`. No publiques el archivo de clave privada. |

Vite también detecta automáticamente `../../BACKEND/.certs/dev-cert.pem` y `../../BACKEND/.certs/dev-key.pem` si existen, con rutas relativas a `vite.config.ts`. El destino del proxy debe comenzar con `http://` o `https://`. El proxy verifica TLS: el certificado de un backend HTTPS debe ser de confianza. Reinicia Vite después de cambiar las variables.

### API keys y secretos

El frontend no requiere ni lee una API key del proveedor del asistente. Envía los mensajes al backend, que gestiona la integración y sus credenciales. Configura las claves reales únicamente en el entorno privado del backend o en el gestor de secretos del despliegue, siguiendo su documentación.

No agregues claves reales al código, al README, a `.env.example` ni a variables `VITE_*`: estas últimas quedan expuestas al navegador. `.gitignore` excluye archivos de entorno privados y conserva `.env.example` como plantilla pública. Si una clave ya fue publicada, revócala y reemplázala; eliminarla del archivo no la elimina del historial de Git.

## Ejecución en desarrollo

Inicia primero el backend y después ejecuta desde este directorio:

```sh
npm run dev
```

Abre `http://localhost:5173`, o la dirección HTTPS indicada por Vite si tienes certificados configurados. El puerto 5173 es fijo: si está ocupado, libera el puerto o ajusta `server.port`. Vite escucha en `0.0.0.0`, por lo que puede ser accesible desde la red local.

El navegador solicita `/api` y `/uploads` al frontend y Vite redirige esas peticiones a `API_PROXY_TARGET`.

## Uso

### Personal

1. Abre `/` e inicia sesión con un usuario creado en el backend. El frontend no proporciona credenciales predeterminadas.
2. Usa el menú lateral para acceder a las secciones disponibles para tu rol.
3. Como administrador, configura categorías, marcas, presentaciones y productos; registra compras para gestionar existencias.
4. Abre la caja antes de registrar o aprobar ventas. Selecciona productos y cliente y completa los pagos según corresponda.
5. Consulta el historial, los pedidos y los reportes desde sus secciones. Cierra la sesión desde el menú de usuario al terminar.

### Clientes

1. Abre `/catalogo` para consultar el catálogo público.
2. Explora categorías, marcas y productos y agrega artículos al carrito.
3. Regístrate o inicia sesión cuando el flujo lo solicite y completa los datos del pedido.
4. Accede al asistente desde el catálogo; sus respuestas requieren conexión y la integración del backend configurada.

Las operaciones y los datos dependen del backend. Consulta también la documentación de [login](docs/login.md) y [ventas](docs/ventas.md).

## Comandos

| Comando | Propósito |
| --- | --- |
| `npm ci` | Instala las versiones de `package-lock.json`. |
| `npm run dev` | Inicia Vite para desarrollo. |
| `npm run build` | Comprueba TypeScript y genera `dist/`. |
| `npm run preview` | Sirve localmente la compilación para inspeccionarla. |
| `npm run lint` | Ejecuta Oxlint. |
| `npm test` | Comprueba TypeScript de pruebas y ejecuta las ocho suites declaradas en el script: autenticación, presentaciones, categorías, marcas, productos, caja, ventas y clientes. |

Hay archivos adicionales en `tests/` que no se ejecutan con el script actual de `npm test`.

## Compilación, despliegue y PWA

```sh
npm run build
npm run preview
```

`preview` permite inspeccionar los archivos compilados, pero no configura el proxy de desarrollo de `server.proxy`. Para probar operaciones de API sobre la compilación necesitas un servidor que dirija `/api` y `/uploads` al backend.

En producción, publica `dist/` mediante HTTPS en la raíz del dominio. Configura el servidor para devolver `index.html` en las rutas del frontend, incluida `/catalogo`, y dirigir `/api` y `/uploads` al backend sin aplicarles ese fallback. `API_PROXY_TARGET` no configura el servidor de producción.

El Dockerfile compila con Node.js 22 y sirve con Nginx:

```sh
 docker build -t sistema-frontend .
```

El `nginx.conf` incluido redirige `/api/` a `http://backend:8080`: requiere ese nombre de servicio en la red de contenedores. Antes del despliegue, agrega el proxy para `/uploads/`, configura HTTPS en el servidor o proxy de entrada y ajusta la caché de la PWA.

El service worker está desactivado en desarrollo. La compilación genera manifiesto, worker e iconos desde `public/icono_principal.jpeg`. No edites los archivos generados en `dist/`.

Para comprobar la PWA, abre la compilación en localhost o HTTPS y revisa el manifiesto y el service worker en DevTools → Application. Tras su activación, prueba el modo Offline. Solo se almacenan recursos estáticos: las peticiones al backend requieren conexión; no se guardan respuestas de API ni operaciones pendientes.

Las actualizaciones se descargan en segundo plano y se activan al cerrar todas las pestañas o ventanas de la aplicación y volver a abrirla. Sirve `sw.js` y el manifiesto sin caché permanente, por ejemplo con `Cache-Control: no-cache`.

## Problemas frecuentes

- **No conecta con el servidor:** comprueba el backend, revisa `API_PROXY_TARGET` y reinicia Vite.
- **Error de certificado:** configura certificado y clave juntos y verifica la confianza del certificado del backend HTTPS.
- **Puerto 5173 ocupado:** detén el proceso que lo usa o ajusta `server.port` en `vite.config.ts`.
- **La API falla en producción:** revisa los proxies de `/api` y `/uploads`; el proxy de Vite solo se aplica al desarrollo.
- **La PWA no muestra cambios:** cierra todas sus ventanas y vuelve a abrirla; durante diagnóstico puedes retirar el worker desde DevTools.
