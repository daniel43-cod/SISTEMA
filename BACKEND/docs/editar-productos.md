# Editar producto

`PUT /api/Productos/{id}` requiere JWT vigente de `ADMINISTRADOR`.

```json
{
  "codigo_barra": "7501234567890",
  "nombre": "Agua 1 litro",
  "idMarca": 1,
  "stock_minimo": 5
}
```

Los cuatro campos son obligatorios. Código: hasta 100 caracteres, sin espacios
internos ni caracteres de control. Nombre: hasta 200 caracteres, sin controles.
Se recortan espacios al inicio/final. Marca y categoría deben estar activas.
Existencia mínima: entero no negativo. No permite enviar campos adicionales.

Nombre y código deben ser únicos entre todos los productos, excluyendo el propio
ID, ignorando mayúsculas y espacios al inicio/final. La comparación adicional de
acentos depende de la intercalación de SQL Server. Crear producto también valida
ahora el nombre para mantener la regla. Verificación y escritura usan una transacción
serializable; un conflicto de concurrencia o interbloqueo inesperado devuelve un
error genérico y no se reintenta automáticamente. No se agregaron índices ni se
cambió el esquema; escrituras externas deben respetar también esta regla.

Conserva stock real, costos, fecha e imagen.

## Presentaciones

El mismo PUT acepta opcionalmente `presentaciones`. Si se omite o es `null`,
conserva la configuración actual. Si envías una lista, las asociaciones existentes
omitidas se desactivan, nunca se borran. Una lista vacía desactiva todas.
Máximo 100 elementos; no se pueden repetir tipos ni IDs.

```json
{
  "codigo_barra": "7501234567890",
  "nombre": "Agua 1 litro",
  "idMarca": 1,
  "stock_minimo": 5,
  "presentaciones": [
    {
      "id_producto_presentacion": 12,
      "id_presentacion": 1,
      "unidades_equivalentes": 1,
      "precio": 5.50,
      "estado": true
    },
    {
      "id_presentacion": 2,
      "unidades_equivalentes": 12,
      "precio": 60.00,
      "estado": true
    }
  ]
}
```

Con `id_producto_presentacion` editas una asociación del producto; sin él agregas
una nueva. Para reactivar una existente debes enviar su ID, no crear otra.
No se permite cambiar `id_presentacion` de una asociación existente ni utilizar
IDs de otro producto. Precio positivo con hasta dos decimales, equivalencia entera
positiva y `estado` explícito. El catálogo debe existir y estar activo para habilitar
la asociación. Las asociaciones inactivas se pueden conservar deshabilitadas.

Datos del producto y presentaciones se guardan en la misma transacción. No cambia
el stock ni reescribe ventas históricas; las referencias mantienen sus IDs.
El PUT devuelve el resumen del producto; consulta `GET /api/Productos/{id}` para
obtener los IDs generados y el detalle actualizado. No hay versión de concurrencia
del formulario: si dos administradores editan consecutivamente, prevalece el último.
`200`: resumen actualizado; `400`: datos inválidos; `401`/`403`: acceso denegado;
`404`: no existe; `409`: nombre o código duplicado; `500`: error genérico con `traceId`.

Servicio: `API_SISTEMA/services/ProductoS/ProductoActualizarService.cs`.
Pruebas HTTP y SQLite en memoria: `tests/ProductoConsulta.Checks`.
