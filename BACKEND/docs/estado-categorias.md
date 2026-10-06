# Estado de categorías (solo administradores)

PATCH /api/Categoria/{idCategoria}/estado, con token de administrador y JSON:

```json
{ "estado": false }
```

false desactiva y true activa. Enviar el estado explícito permite repetir la petición
sin invertirlo accidentalmente. Estado omitido/null, tipos incorrectos, ID no positivo
o campos desconocidos devuelven 400. Categoría inexistente: 404. Sin sesión: 401;
vendedor: 403. Respuesta 200 con ID, nombre, estado e imagen.

GET /api/Categoria/administracion devuelve activas e inactivas solo al administrador,
para permitir reactivarlas. GET /api/Categoria sigue devolviendo únicamente activas.

Se actualiza solo el estado: no se borran categorías ni se cambian marcas o productos
asociados. No se necesita una columna nueva; se usa estado existente.
