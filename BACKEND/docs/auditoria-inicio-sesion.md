# Auditoría de inicio de sesión interno

Requiere ejecutar previamente `sql/auditoria-eventos.sql` en SISTEMA.
Cada autenticación exitosa en POST /api/Login/Login guarda SESION_INICIADA con
responsable obtenido de la cuenta validada, fecha UTC, entidad usuario, resultado
EXITOSO, TraceId e IP de la conexión. No guarda contraseñas, hashes ni tokens.

Login es anónimo: no se usa el usuario de un JWT previo para atribuir el evento.
ContextoPeticion vive en Securyti y proporciona metadatos del servidor.
AuditoriaService vive en services/Auditoria. Ambos son scoped.

El token solo se entrega tras persistir el evento. Si falla el almacenamiento se
devuelve el error interno controlado existente. El evento acredita autenticación
aceptada; no garantiza que el cliente haya recibido la respuesta.

Los intentos con credenciales incorrectas, cuenta inexistente/inactiva o rol no permitido
registran LOGIN_FALLIDO, resultado RECHAZADO, identificador intentado y responsable NULL.
Se conserva el mensaje genérico al cliente. Si no se puede guardar el evento, se devuelve
el error interno controlado. No se registran como intentos de autenticación las peticiones
rechazadas por validación del DTO (400) o límite de solicitudes (429), que no llegan a
verificar credenciales. Los errores de infraestructura tampoco se etiquetan como contraseña incorrecta.

POST /api/Login/cerrar-sesion exige autenticación de administrador o vendedor y
devuelve 204 después de guardar SESION_CERRADA. El responsable se obtiene del token
validado y de la cuenta en la BD; no se acepta un ID del cliente.
El botón de salir notifica ese endpoint y limpia inmediatamente la sesión local.
Si falla la notificación muestra que no se pudo confirmar el registro en el servidor.
Expiración, respuestas 401 y cierre de pestaña no se etiquetan como cierre voluntario.
Este endpoint registra la solicitud de cierre; no revoca el JWT, que conserva su
vigencia hasta expirar. Repetir la petición puede generar otro evento de cierre.

Este cambio no registra cambios de catálogos.
No se necesitan triggers ni se ejecutan modificaciones de esquema automáticamente.

Verificación: tests/Auth.Checks usa SQLite para probar el flujo; adapta tipos JSON y
restricciones específicas de SQL Server. No verifica la instalación real en SQL Server.
