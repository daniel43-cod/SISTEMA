# Estado de presentaciones del catálogo

Solo administradores con JWT interno vigente:

`PATCH /api/Presentaciones/{id}/estado`

```json
{ "estado": false }
```

`true` activa y `false` desactiva. Campo obligatorio, booleano, sin campos extra.
`200` devuelve ID, descripción y estado. `400`: entrada inválida; `401`/`403`:
acceso denegado; `404`: no existe; `500`: error genérico con traceId.
Repetir el mismo estado es válido. No elimina registros ni modifica asociaciones
de productos ni ventas anteriores. Reactivar vuelve a habilitar las asociaciones
que mantengan su propio estado activo.

`GET /api/Presentaciones/administracion` incluye activas e inactivas, exclusivo
del administrador para permitir reactivación. El GET original conserva solo activas.
Las ventas nuevas y la reconfiguración de detalles de ventas exigen que tanto
la asociación del producto como la presentación del catálogo estén activas.
El frontend todavía debe conectar sus controles a estas rutas.
