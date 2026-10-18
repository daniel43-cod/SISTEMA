# Creación de usuarios internos

`Services/Usuarios/UsuarioCrearService.cs` implementa el alta. `UsuarioService.CrearUsuario`
delega en él para conservar compatibilidad. Las rutas `POST /api/Usuario` y
`POST /api/Login/crear` siguen restringidas a administradores.

- Usuario y teléfono obligatorios y únicos. Correo opcional: vacío o espacios se convierten en null.
- Los índices únicos existentes en SQL Server protegen las altas concurrentes.
  Para admitir varios usuarios sin correo, su índice único debe estar filtrado con
  `WHERE correo IS NOT NULL`: una restricción UNIQUE normal de SQL Server admite un solo null.
  Este cambio no ejecuta SQL ni modifica los índices existentes.
- Las comparaciones de duplicados respetan la intercalación de SQL Server.
- Teléfono: números, prefijo y separadores habituales; se recortan espacios externos.
  No se unifican formatos diferentes del mismo número.
- Contraseña BCrypt, validación de rol interno activo y cancelación propagada a EF Core.
- Usuario y evento USUARIO_CREADO se confirman juntos. El evento identifica al administrador
  y guarda usuario, rol y estado, sin contraseña ni hash.
- Duplicados: HTTP 409. Validación: HTTP 400. Error inesperado: HTTP 500 sin datos internos.
- Ambas rutas comparten 30 intentos por minuto por administrador, con HTTP 429 y Retry-After.
  El contador está en memoria de cada instancia; múltiples réplicas requieren un límite compartido.

Pruebas: `dotnet run --project tests/Auth.Checks/Auth.Checks.csproj`.
