# Consulta administrativa de productos

Ambas rutas requieren un JWT interno vigente con rol `ADMINISTRADOR`.
La validación existente rechaza cuentas/roles inactivos y credenciales modificadas.
Anónimo o JWT inválido: `401`. Vendedor o cliente autenticado: `403`.
Las respuestas llevan `Cache-Control: no-store`.
Listado y detalle comparten un límite de 60 consultas por minuto por usuario
en cada proceso de la API, sin cola. Al excederlo: `429` con `Retry-After`.
Un despliegue con varias instancias requiere un límite compartido en el proxy
o almacenamiento distribuido para aplicar el mismo presupuesto global.

## Listado

`GET /api/Productos/listar?pagina=1&tamanoPagina=20&texto=agua&idMarca=1&idCategoria=1`

Todos los parámetros son opcionales. Página: 1–100000; tamaño: 1–100,
predeterminado 20; texto: máximo 100 caracteres; IDs: enteros positivos.
El texto se recorta y busca por nombre o código de barras. Los filtros se combinan.
Entradas inválidas: `400`. No se aceptan expresiones SQL ni ordenamiento dinámico;
EF parametriza los valores de búsqueda. No hay filtro de estado del producto,
porque el modelo actual no tiene ese campo.

Respuesta: `{ pagina, tamanoPagina, total, items }`. Cada item contiene
`idProducto`, `codigoBarra`, `nombre`, `imagen`, `stockUnidades`, `stockMinimo`,
`idMarca`, `marca`, `marcaActiva`, `idCategoria`, `categoria`, `categoriaActiva`.
Una fila por producto, incluyendo productos sin presentaciones y marcas/categorías
inactivas para administración. Orden por nombre e ID. Una página vacía devuelve
`200` con `items: []`. Conteo y página se consultan por separado; cambios concurrentes
pueden hacer variar el total o mover elementos entre páginas.

**Cambio de contrato:** la ruta antes devolvía un arreglo de presentaciones.
El frontend nuevo debe leer `items` y usar `idProducto` para abrir el detalle.
Las rutas de búsqueda para ventas y listado de presentaciones conservan su contrato.

## Detalle

`GET /api/Productos/123`

Devuelve los campos del resumen más `fechaCreacion` y `presentaciones`.
Cada presentación contiene `idProductoPresentacion`, `idPresentacion`,
`descripcion`, `unidadesEquivalentes`, `precio`, `activa`, `presentacionActiva`
y `presentacionesDisponibles`. Incluye presentaciones inactivas para administración.
Disponibilidad: división entera del stock en unidades por equivalencia; cero si
está inactiva la asociación, la presentación, la marca o la categoría, si no hay
stock o si la equivalencia no es positiva. No reserva stock ni garantiza que
la cantidad siga disponible al vender.

ID no positivo: `400`; ID inexistente: `404`. No devuelve costos de compra,
ganancias, usuarios ni navegaciones EF. Error inesperado: `500` con mensaje
genérico y `traceId`; detalle técnico solo en el registro del servidor.
Las consultas propagan la cancelación del cliente a EF.

## Comprobaciones

```powershell
dotnet run --project tests/ProductoConsulta.Checks/ProductoConsulta.Checks.csproj --artifacts-path .artifacts/producto-consulta -p:UseAppHost=false
dotnet run --project tests/Auth.Checks/Auth.Checks.csproj --artifacts-path .artifacts/auth -p:UseAppHost=false
```

La primera usa SQLite en memoria y HTTP local con JWT firmados de prueba para
comprobar autorización por rol, validación, paginación, filtros, detalle y errores.
La segunda comprueba la validación de cuentas y JWT internos existente.
No consumen la base SQL Server del negocio. El esquema real debe probarse
manualmente en desarrollo con un administrador antes de publicar.
