# Estado de marcas (solo administradores)

PATCH /api/Marcas/{idMarca}/estado con token y JSON:

```json
{ "estado": false }
```

false desactiva; true activa. La operación asigna un estado explícito y es idempotente.
Respuesta 200 con ID, nombre, categoría, estado e imagen. Estado omitido/null, tipos
incorrectos, campos desconocidos e ID no positivo: 400. Marca inexistente: 404.
Sin sesión: 401; vendedor: 403.

GET /api/Marcas/administracion incluye activas e inactivas, solo para administradores.
GET /api/Marcas conserva su comportamiento de devolver únicamente marcas activas.

Se usa la columna estado existente. No se eliminan marcas ni se cambian sus productos
o categoría. El estado de la categoría sigue siendo independiente: activar una marca
no reactiva su categoría. El frontend no se modifica en este cambio.
