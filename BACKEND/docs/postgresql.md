# Instalación con PostgreSQL 18

El sistema usa exclusivamente PostgreSQL mediante Npgsql y EF Core 10. La migración inicial crea una base vacía: no importa usuarios, ventas ni otros datos de prueba. La API no aplica migraciones ni crea cuentas automáticamente.

## Desarrollo local

1. Crear una base vacía y un usuario propietario de ella (sin superusuario). Usar el nombre exacto creado en pgAdmin.
2. Desde BACKEND, instalar herramientas y dependencias:

```powershell
dotnet tool restore
dotnet restore API_SISTEMA/API_SISTEMA.csproj
```

3. Guardar `ConnectionStrings:ConexionPostgres` en User Secrets del proyecto API_SISTEMA. Formato:

```text
Host=localhost;Port=5432;Database=sistema_desarrollo;Username=sistema_app;Password=TU_CLAVE
```

No guardar contraseñas en Git ni en appsettings. También configurar Jwt:Key (mínimo 32 bytes), Jwt:Issuer y Jwt:Audience. User Secrets sirve para desarrollo y no cifra sus valores.

4. Revisar el destino y generar el SQL antes de aplicar:

```powershell
dotnet ef migrations script --project API_SISTEMA --startup-project API_SISTEMA --context SistemaDbContext --output .tmp/revisar-postgres.sql
dotnet ef database update --project API_SISTEMA --startup-project API_SISTEMA --context SistemaDbContext
```

`database update` crea las tablas en la conexión configurada. No ejecutarlo sobre otra aplicación, como n8n. `docs/sql/inicial-postgres.sql` es una alternativa generada de la misma migración: utilizar EF o el SQL, no ambos como instalaciones independientes.

5. En pgAdmin, conectado a esa base con su usuario, ejecutar `docs/sql/datos-iniciales.sql`. Crea roles, catálogo de permisos, estados y tipos de movimientos con los identificadores que usa la aplicación. Es repetible y no crea usuarios, productos ni operaciones de prueba. Solo el rol administrador recibe los permisos iniciales; los permisos del vendedor se asignan desde el panel.

6. Crear el administrador inicial mediante la herramienta explícita. Configurar en los mismos User Secrets de API_SISTEMA (o variables de entorno Bootstrap__...):

- Bootstrap:Usuario, Bootstrap:Nombre, Bootstrap:Apellido, Bootstrap:Telefono.
- Bootstrap:Correo (opcional).
- Bootstrap:Password: contraseña que cumpla las reglas del formulario de usuarios.
- Bootstrap:NombreCaja: nombre elegido para la caja inicial.

```powershell
dotnet run --project tools/InicializarSistema
```

La herramienta se niega a ejecutarse si ya hay usuarios. Valida los campos, guarda un hash BCrypt y registra auditoría dentro de la transacción. Eliminar Bootstrap:Password después de usarla. No se incluye ninguna contraseña predeterminada.

7. Iniciar la API normalmente. El asistente y las cuentas públicas continúan desactivados.

## Tipos y concurrencia

- Ventas, compras, pagos, caja y catálogo interno: `timestamp without time zone`, hora civil de Guatemala.
- Auditoría, sesiones, pedidos públicos y marcas UTC: `timestamp with time zone`.
- Fechas fiscales DateTimeOffset se normalizan al mismo instante UTC; el XML conserva el documento original.
- `xmin` protege las actualizaciones de sesiones y documentos FEL.
- Un índice único `NULLS NOT DISTINCT`, filtrado por cierre nulo, permite una única caja abierta.
- Claves únicas de operación protegen ventas y abonos. Fallos de serialización o bloqueo devuelven conflicto; el reintento conserva la clave.
- `citext` conserva comparaciones de usuario/correo insensibles a mayúsculas; no implica que toda búsqueda de productos ignore acentos o mayúsculas.
- Extensión `citext` instalada por la migración. Es una extensión confiable; el propietario de la base debe poder crearla.

## Docker Compose

Desde la carpeta SISTEMA (padre de BACKEND), completar `.env` siguiendo `.env.example`. Las conexiones dentro de Compose utilizan `Host=postgres`, no `localhost`. La contraseña de POSTGRES_CONNECTION_STRING debe coincidir con POSTGRES_PASSWORD y estar correctamente escapada si contiene delimitadores de cadena.

- `compose.yaml`: imágenes compiladas para pruebas locales; sigue usando Development y puertos loopback.
- `compose.dev.yaml`: código montado y recarga de desarrollo. El montaje de User Secrets indicado es para Windows.
- PostgreSQL 18 persiste en el volumen montado en `/var/lib/postgresql`.
- POSTGRES_ADMIN_PASSWORD es la clave del administrador del contenedor. POSTGRES_USER y POSTGRES_PASSWORD son del usuario de aplicación; el script de inicio lo crea sin superusuario y le asigna la base. Este script solo se ejecuta cuando el volumen está vacío.
- El backend compilado persiste cargas en `/app/wwwroot/uploads`.
- Iniciar PostgreSQL primero, aplicar migraciones y datos iniciales explícitamente, luego iniciar la API. Para migrar desde el host a una base Docker se requiere una publicación de puerto local explícita o un contenedor SDK en la misma red; no reutilizar una conexión a otra base.
- Los archivos Docker no constituyen una configuración completa de VPS: aún hay que definir dominio, TLS, proxy confiable, respaldos y configuración de Production.

Respaldar con pg_dump y respaldar también las imágenes. Guardar copias fuera del servidor y ensayar su restauración. El volumen no es un respaldo.

## Comprobaciones

`tests/Postgres.Checks` genera el esquema completo sin conectarse por defecto. `tests/PostgresCatalogo.Checks` revisa catálogo y productos. Las demás suites de flujo usan SQLite o HTTP simulado.

La integración real solo se habilita con POSTGRES_TEST_CONNECTION y exige `Host=127.0.0.1;Port=55438;Database=sistema_checks`: un clúster temporal aislado. No utilizar credenciales de la aplicación ni de una base del usuario. Verifica migraciones, caja única, ventas, compras, pedidos, idempotencia simultánea, precisión, fechas, xmin y bloqueos de fila.