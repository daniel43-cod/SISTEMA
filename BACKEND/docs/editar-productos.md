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

Conserva stock real, costos, fecha, imagen y presentaciones. Estas últimas tienen
su propio historial de ventas y no se reemplazan en esta operación.
`200`: resumen actualizado; `400`: datos inválidos; `401`/`403`: acceso denegado;
`404`: no existe; `409`: nombre o código duplicado; `500`: error genérico con `traceId`.

Servicio: `API_SISTEMA/services/ProductoS/ProductoActualizarService.cs`.
Pruebas HTTP y SQLite en memoria: `tests/ProductoConsulta.Checks`.
