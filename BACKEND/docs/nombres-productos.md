# Nombres de productos para búsqueda local

`GET /api/Productos/nombres` requiere JWT interno vigente de ADMINISTRADOR.
Devuelve todos los productos, una fila por producto, ordenados por nombre e ID:

```json
[{ "idProducto": 1, "nombre": "Coca Cola 2.5" }]
```

Sin paginación ni filtros: pensado para el catálogo actual de unos 500 productos.
Solo consulta ID y nombre; no carga presentaciones, marca, imágenes, costos ni stock.
Catálogo vacío: `200` con `[]`. Acceso denegado: `401`/`403`. Error inesperado:
`500` con mensaje genérico y traceId. Comparte el límite de consultas de productos
de 60 por minuto por usuario y proceso, con `429` y Retry-After.
Respuesta `no-store`: la memoria temporal del frontend se maneja explícitamente,
no mediante caché HTTP ni almacenamiento persistente.

El frontend debe cargar una vez al entrar, filtrar localmente y consultar
`GET /api/Productos/{id}` al seleccionar. Refrescar después de altas/ediciones,
permitir actualización manual ante cambios de otros usuarios y vaciar al cerrar
sesión. La búsqueda por código sigue usando `buscar-administracion?codigoBarra=...`.
Este cambio agrega solo el endpoint; la búsqueda local del frontend queda pendiente.
