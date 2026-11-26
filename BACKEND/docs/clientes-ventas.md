# Clientes en Ventas

GET /api/Cliente/listar devuelve únicamente id_cliente y nombre (nombre completo para la búsqueda local). No excluye clientes inactivos ni impone un máximo de clientes. El frontend conserva un índice de esos dos campos en el estado de la pantalla; no usa localStorage ni guarda fichas completas de todos los clientes.

GET /api/Cliente/{id} consulta el detalle solo al seleccionar: id_cliente, nombre, apellido, NIT, DPI, teléfono, correo y dirección. Los campos se llenan automáticamente y son de lectura en Ventas para clientes existentes. Para registrar una venta existente se envía su ID; los datos no se modifican desde esta pantalla. La creación de cliente nuevo conserva su flujo.

Las dos consultas requieren ADMINISTRADOR o VENDEDOR, usan proyecciones DTO, AsNoTracking y CancellationToken. Los errores internos se registran en el servidor y no se devuelven al navegador. Un cliente inexistente devuelve 404.

Se eliminan el método duplicado ListarCliente, el endpoint /Cliente/buscar y ClienteBuscarDto, ya sin consumidores en el proyecto. La búsqueda por nombre se realiza en memoria. Si existen aplicaciones externas que llamaban a /Cliente/buscar, deberán migrar a este contrato.

El selector cancela la consulta previa al escribir, seleccionar otra persona o cambiar a cliente nuevo; ignora respuestas antiguas y no habilita el registro mientras falten los datos seleccionados. El índice en memoria contiene solo ID y nombre; únicamente los campos del cliente seleccionado permanecen en el formulario.

No requiere cambios SQL ni ejecuta ventas para probar el listado.

## Módulo administrativo de clientes

Las consultas nuevas no modifican el selector de Nueva venta. Todas requieren un JWT de ADMINISTRADOR y comprueban además que el usuario y su rol sigan activos en la base de datos. No dependen de tener una caja abierta.

- `GET /api/clientes?buscar=Maria&pagina=1&tamanoPagina=30`: búsqueda parcial por nombre completo, NIT o teléfono. Devuelve `totalRegistros`, `pagina`, `tamanoPagina`, `hayMas` e `items` con `idCliente`, `nombre`, `nit`, `telefono` y `estado`. Incluye clientes inactivos y clientes sin ventas.
- `GET /api/clientes/{idCliente}/resumen`: devuelve `cliente` (datos personales), `estado`, `cantidadVentas`, `totalComprado`, `totalPagado` y `saldoPendiente`. Los importes se agregan en PostgreSQL sobre todas las ventas vinculadas al ID del cliente, sin limitar por página, responsable o caja. Un cliente sin ventas obtiene totales cero.
- `GET /api/clientes/{idCliente}/ventas?pagina=1&tamanoPagina=30&fecha=2026-11-01`: devuelve `idCliente`, `totalRegistros`, `pagina`, `tamanoPagina`, `hayMas` e `items` con `idVenta`, `fecha`, `idUsuario`, `responsable`, `total`, `pagado`, `saldo`, `idEstadoVenta` y `estado`. Incluye cajas abiertas y cerradas. Orden: fecha descendente, luego ID descendente. El ID de venta permite abrir sus detalles y abonos mediante los endpoints existentes.

Paginación: 30 registros por defecto, entre 1 y 100 por página; página entre 1 y 100000. Búsqueda de hasta 100 caracteres. Un cliente inexistente responde 404; parámetros inválidos, 400; vendedor o usuario desactivado, 403. No requiere migración. Las consultas no fusionan registros de clientes duplicados: la vinculación es por `id_cliente`.

Pruebas: `tests/Clientes.Checks` ejecuta las consultas sobre PostgreSQL aislado (`127.0.0.1:55438/sistema_checks`) dentro de una transacción que se revierte. No usar la base del negocio.

El historial muestra por defecto los últimos 30 días naturales, incluyendo hoy según America/Guatemala. El parámetro opcional fecha (yyyy-MM-dd) limita la consulta al día elegido completo, con límite final exclusivo al día siguiente; permite días antiguos y rechaza fechas futuras. La paginación se aplica después del filtro. El resumen financiero conserva los totales históricos del cliente. El calendario permite borrar la fecha o usar Últimos 30 días para regresar a la vista inicial.

## Edición administrativa
PUT /api/clientes/{idCliente} actualiza únicamente nombre, apellido, NIT, DPI, teléfono, correo y dirección. Requiere administrador y rol activos, como las consultas. Nombre es obligatorio (hasta 100 caracteres); apellido hasta 100; NIT/DPI hasta 20; teléfono opcional de exactamente 8 dígitos ASCII, sin espacios, letras ni prefijo +502; correo hasta 254 con formato válido si se informa; dirección hasta 200. Se recortan espacios y los opcionales vacíos se guardan como null. Devuelve el resumen actualizado. No cambia ID, estado, fecha de creación, límite de crédito, ventas, pagos ni datos fiscales almacenados en cada venta. No requiere migración. El formulario conserva los datos si falla el guardado y actualiza el detalle tras confirmar la respuesta; al regresar al listado se vuelve a consultar.
DPI y NIT en la edición aceptan únicamente dígitos ASCII o vacío; conservan ceros iniciales. Por decisión del negocio, este formulario no acepta K, guiones ni otras letras en NIT. La validación se aplica en la API y en el frontend; no modifica registros existentes automáticamente.
