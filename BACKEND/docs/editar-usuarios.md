# Editar usuarios

`PUT /api/Usuario/{id}` requiere sesión de administrador.

```json
{
  "nombre": "Ana",
  "apellido": "López",
  "usuario": "ana",
  "corre_electronico": null,
  "telefono": "5555-1234",
  "id_rol": 2
}
```

Enviar el ID real de un rol interno activo (ADMINISTRADOR o VENDEDOR).
Es una actualización completa de estos campos: omitir el correo o enviarlo vacío lo elimina.
Contraseña, estado, ID y fecha de creación se conservan.

Devuelve 200 con UsuarioRespuestaDto sin contraseña ni hash, 400 por validaciones,
401 sin sesión válida, 403 sin rol administrador, 404 si no existe el usuario y
409 por usuario, correo o teléfono duplicados. La comprobación de duplicados excluye
al propio usuario y los índices únicos de SQL Server resuelven las carreras concurrentes.

Servicio: Services/Usuarios/UsuarioEditarService.cs. Cambios y auditoría USUARIO_EDITADO
se guardan en una transacción. Guardar sin cambios no genera un evento de auditoría.
El endpoint comparte el límite de 30 solicitudes por minuto de creación por administrador
(por instancia). Propaga CancellationToken.

Cambiar el rol invalida los tokens anteriores mediante UsuarioTokenValidator. Si un
administrador cambia su propio rol deberá volver a iniciar sesión.
