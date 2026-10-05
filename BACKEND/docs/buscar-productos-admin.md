# Búsqueda administrativa

Solo ADMINISTRADOR con JWT vigente. No cambia la búsqueda de ventas.

- `GET /api/Productos/buscar-administracion?nombre=Coca`: busca por inicio del nombre.
- `GET /api/Productos/buscar-administracion?codigoBarra=7501234567890`: código completo exacto.

Enviar uno de los dos parámetros. Nombre: 3–200 caracteres; código: hasta 100,
sin espacios internos ni controles. Se recortan espacios exteriores.
Coincidencias de mayúsculas y acentos dependen de la intercalación SQL Server.
`coca c` coincide con nombres que comiencen por ese texto, no con `Coca 3 litros`.
Devuelve hasta 10 productos únicos, ordenados por nombre e ID:

```json
[{ "idProducto": 1, "nombre": "Coca Cola 2.5" }]
```

Sin coincidencias: `200` y `[]`. Entrada inválida: `400`; acceso: `401`/`403`.
Comparte el límite de consultas de productos: `429` y `Retry-After`.
No devuelve presentaciones ni ejecuta conteo total. Cancelación propagada a EF.
Al seleccionar, consultar `GET /api/Productos/{id}` para renderizar el producto
con sus acciones de edición y detalles. Búsqueda global, sin filtros de marca
o categoría ni exclusión de inactivos, por ser administrativa.

El frontend debe esperar 350 ms sin escritura, exigir tres caracteres para
nombre, cancelar solicitudes anteriores y reutilizar brevemente búsquedas iguales.
Código: buscar con Enter o al completar el escaneo. Eso aún no se implementa aquí.
Verificar índices de nombre/código y plan de ejecución en SQL Server antes de
afirmar tiempos de respuesta; no se cambió el esquema ni se midió la base real.
