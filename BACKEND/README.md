# Sistema de gestión comercial

Aplicación para administrar productos, inventario, compras, ventas, caja, gastos, usuarios y reportes. Incluye un catálogo público, pedidos y un asistente de consultas con IA.

El frontend está desarrollado con React 19, TypeScript y Vite 8, con soporte PWA. El backend es una API REST en C# con ASP.NET Core (.NET 10), Entity Framework Core, SQL Server y autenticación JWT.

## Organización del código fuente

Las rutas siguientes parten de la carpeta `SISTEMA` que contiene ambos proyectos:

```text
SISTEMA/
├── BACKEND/
│   ├── API_SISTEMA.slnx          # Solución del backend
│   ├── API_SISTEMA/
│   │   ├── Controllers/         # Endpoints de la API
│   │   ├── Services/            # Lógica de negocio por módulo
│   │   ├── Models/              # Entidades del sistema
│   │   ├── DTOs/                # Datos de entrada y salida
│   │   ├── Data/                # Contextos y acceso a SQL Server
│   │   ├── Security/            # Contexto de seguridad
│   │   ├── Utilidades/          # JWT y utilidades compartidas
│   │   ├── Filters/             # Filtros de peticiones
│   │   ├── Exceptions/          # Excepciones de la aplicación
│   │   ├── wwwroot/             # Archivos estáticos y cargas
│   │   ├── Program.cs           # Servicios y configuración HTTP
│   │   └── appsettings.json     # Configuración base
│   ├── docs/                   # Documentación técnica y ajustes SQL
│   ├── scripts/                # Herramientas para desarrollo HTTPS
│   └── tests/                  # Proyectos de comprobación
└── Front-End.web/
    └── Front-End/
        ├── src/
        │   ├── app/            # Aplicación, proveedores y layouts
        │   ├── features/       # Funcionalidades por módulo
        │   ├── shared/         # Recursos y componentes compartidos
        │   ├── assets/         # Recursos visuales
        │   ├── icons/          # Iconos
        │   ├── styles/         # Estilos compartidos
        │   └── main.tsx        # Punto de entrada
        ├── public/             # Recursos públicos y de la PWA
        ├── tests/              # Pruebas del frontend
        ├── package.json        # Dependencias y comandos
        ├── vite.config.ts      # Servidor, proxy y PWA
        └── .env.example        # Ejemplo de configuración local
```

## Instalación

### Requisitos

- SDK de .NET 10.
- Node.js `20.19+` de la rama 20 o `22.12+`, y npm, según el requisito de Vite instalado.
- SQL Server accesible y una base de datos con el esquema y los datos iniciales del sistema.
- Una cuenta interna habilitada para ingresar al panel.

### Backend

Desde `BACKEND`, restaurar las dependencias:

```powershell
dotnet restore .\API_SISTEMA\API_SISTEMA.csproj
```

Preparar la base de datos a partir del respaldo o esquema proporcionado por el responsable del proyecto. Los archivos de `docs/sql/` son ajustes específicos; no constituyen un instalador completo de la base de datos. El inicio de la API no crea ni migra automáticamente la base.

Configurar la conexión y JWT en la terminal donde se ejecutará la API:

```powershell
$env:ConnectionStrings__ConexionSQL = 'Server=localhost\SQLEXPRESS;Database=SISTEMA;Trusted_Connection=True;TrustServerCertificate=True;'
$jwtBytes = New-Object byte[] 32
$jwtGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$jwtGenerator.GetBytes($jwtBytes)
$jwtGenerator.Dispose()
$env:Jwt__Key = [Convert]::ToBase64String($jwtBytes)
$env:Jwt__Issuer = 'SistemaPV'
$env:Jwt__Audience = 'SistemaPVUsuarios'
$env:Jwt__DurationInMinutes = '720'
```

Adaptar el servidor, la base y la autenticación a la instalación de SQL Server. La clave del ejemplo se genera para esa sesión; para conservar la validez de los tokens entre reinicios, guardar una clave estable mediante secretos de usuario de .NET o la configuración segura del entorno.

### Frontend

Desde `BACKEND`, abrir otra terminal y ejecutar:

```powershell
cd ..\Front-End.web\Front-End
npm ci
Copy-Item .env.example .env.local
```

El archivo `.env.local` debe contener el destino de la API:

```dotenv
API_PROXY_TARGET=http://localhost:5272
```

## Ejecución

### Backend

En la terminal de `BACKEND` donde se configuraron las variables:

```powershell
dotnet run --project .\API_SISTEMA\API_SISTEMA.csproj --launch-profile http
```

- API: `http://localhost:5272`.
- Swagger: `http://localhost:5272/swagger` (habilitado en Development).

### Frontend

En la terminal de `Front-End.web/Front-End`:

```powershell
npm run dev
```

Abrir la dirección que muestra Vite; normalmente `http://localhost:5173`. El servidor reenvía `/api` y `/uploads` al backend mediante el proxy configurado.

Si existen los archivos `BACKEND/.certs/dev-cert.pem` y `dev-key.pem`, Vite activa HTTPS automáticamente. Para ese escenario, seguir [la guía HTTPS](docs/https-desarrollo.md), iniciar la API con el perfil `https` y ajustar `API_PROXY_TARGET` a `https://localhost:7272`.

### Compilación

Backend, desde `BACKEND`:

```powershell
dotnet build .\API_SISTEMA\API_SISTEMA.csproj
dotnet publish .\API_SISTEMA\API_SISTEMA.csproj -c Release -o .\publish
```

Frontend, desde `Front-End.web/Front-End`:

```powershell
npm run build
npm run preview
```

La compilación del frontend genera `dist/`. El proxy se configura en `server.proxy` para desarrollo; al servir la compilación, configurar el servidor web para dirigir `/api` y `/uploads` a la API y resolver las rutas de la interfaz mediante `index.html`.

## Uso

1. Iniciar SQL Server, el backend y el frontend.
2. Abrir el frontend e ingresar con una cuenta interna existente. Las opciones disponibles dependen de los permisos del usuario.
3. Administrar categorías, marcas, presentaciones y productos; consultar inventario y registrar las operaciones de compras, caja, ventas y gastos según los permisos asignados.
4. Consultar los reportes desde el panel.
5. Abrir `/catalogo` en el frontend para consultar el catálogo público y acceder al flujo de pedidos.

Para probar endpoints directamente, abrir Swagger, ejecutar el inicio de sesión y copiar el JWT recibido en **Authorize**. Swagger espera únicamente el token; en otras herramientas enviar `Authorization: Bearer <token>` para las rutas protegidas.

El asistente requiere las variables de IA del backend. La PWA permite cargar recursos de la interfaz sin conexión después de instalar su caché; las consultas y operaciones de la API necesitan conexión.

## Variables de entorno

### Backend

ASP.NET Core lee variables del proceso y permite reemplazar las claves de `appsettings.json` usando `__` como separador de niveles. El proyecto no carga automáticamente un archivo `.env`: definir las variables en la terminal, en el entorno de ejecución o usar secretos de usuario de .NET para desarrollo.

| Variable | Uso | Requerida / valor base |
| --- | --- | --- |
| `ConnectionStrings__ConexionSQL` | Conexión a SQL Server. | Se requiere una conexión válida. Reemplazar la conexión de ejemplo de `appsettings.json` para cada instalación. |
| `Jwt__Key` | Clave para firmar y validar JWT. | Obligatoria; al menos 32 bytes UTF-8. No está definida en `appsettings.json`. |
| `Jwt__Issuer` | Emisor del token. | Base: `SistemaPV`. |
| `Jwt__Audience` | Destinatario del token. | Base: `SistemaPVUsuarios`. |
| `Jwt__DurationInMinutes` | Duración del token en minutos. | Base: `720`; debe ser mayor que cero. |
| `OPENAI_API_KEY` | Clave del proveedor para el asistente de IA. | Obligatoria para utilizar el asistente. |
| `OPENAI_MODEL` | Identificador del modelo utilizado por el asistente. | Obligatoria para utilizar el asistente; no hay valor predeterminado. |
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución. | Los perfiles locales establecen `Development`. |
| `ASPNETCORE_URLS` | Direcciones donde escucha la API. | Opcional si se usan los perfiles locales. |
| `Kestrel__Certificates__Default__Path` | Ruta del certificado HTTPS. | El perfil `https` define `../.certs/dev-cert.pem`. |
| `Kestrel__Certificates__Default__KeyPath` | Ruta de la clave privada HTTPS. | El perfil `https` define `../.certs/dev-key.pem`. |
| `AllowedHosts` | Hosts aceptados por la API. | Base: `*`; ajustar según el entorno. |

Ejemplo opcional para habilitar el asistente en la misma terminal de la API:

```powershell
$env:OPENAI_API_KEY = '<clave-del-proveedor>'
$env:OPENAI_MODEL = '<identificador-del-modelo>'
```

### Frontend

Vite lee estas variables desde `.env.local` en `Front-End.web/Front-End` o desde el proceso. Reiniciar el servidor después de cambiarlas.

| Variable | Uso | Valor predeterminado |
| --- | --- | --- |
| `API_PROXY_TARGET` | Destino del proxy de desarrollo para `/api` y `/uploads`. | `http://localhost:5272`, o `https://localhost:7272` cuando Vite usa HTTPS. |
| `DEV_HTTPS_CERT` | Ruta al certificado del servidor local de Vite. | Busca `BACKEND/.certs/dev-cert.pem`. |
| `DEV_HTTPS_KEY` | Ruta a la clave privada del servidor local de Vite. | Busca `BACKEND/.certs/dev-key.pem`. |

Configurar certificado y clave juntos para HTTPS. Estas variables son utilizadas por la configuración de Vite y no necesitan prefijo `VITE_`. No colocar contraseñas, claves JWT ni claves de IA en el frontend o en variables `VITE_`, porque los valores expuestos al cliente pueden incluirse en la compilación. No versionar archivos con secretos.
