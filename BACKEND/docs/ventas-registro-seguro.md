# Registro de ventas: despliegue y reglas

## Cambio de esquema pendiente

El script docs/sql/ventas-registro-seguro.sql está preparado pero NO fue ejecutado. Antes de desplegar este código, revisar el esquema real, respaldar y aplicar ese script con autorización del responsable. La nueva API depende de venta_operacion y de detalle_venta.costo_unitario_venta. No arrancar este cambio contra la base anterior esperando que EF la actualice automáticamente.

## Reglas confirmadas

- Administrador y vendedor pueden dar descuentos; deben conservar el permiso de vender.
- Solo EFECTIVO. Crédito sin límite y pago parcial o cero permitidos.
- El estado del cliente no bloquea una venta.
- Marca, categoría y presentación deben estar activas.
- Sin costo promedio positivo no se permite vender.
- El importe final de CADA línea debe superar estrictamente el costo promedio multiplicado por las unidades base. Costo Q100, precio Q120: descuento Q19 válido; Q20/Q21 inválidos.
- Descuento es un importe por línea, no un porcentaje ni descuento por unidad. Se conserva el significado del DTO anterior.
- Descuentos y pagos admiten hasta dos decimales. Sin redondeo silencioso de esos importes.

## Endpoints y compatibilidad

POST /api/Venta/crear exige id_operacion (UUID). Conservar la misma clave y el mismo contenido en un reintento. Devuelve id_venta, id_operacion, subtotal, descuento, total, monto_pagado, saldo_pendiente y fecha_venta. Repeticiones de otra identidad o contenido producen 409. La restricción de clave única protege también múltiples instancias; ante conflicto transaccional, consultar o reintentar con la MISMA clave. No hay reintentos automáticos.

GET /api/Venta/listar admite pagina, tamanoPagina (máximo 100), desde y hasta. GET /api/Venta/catalogo admite pagina y tamanoPagina (máximo 100). Se conserva la respuesta como lista, ahora paginada. El frontend se adaptó para enviar claves, cargar páginas de catálogo y navegar el historial. Mantiene la misma clave al recuperar una petición incierta. El catálogo excluye presentaciones inactivas y productos sin costo válido o margen positivo.

No se ejecuta ninguna certificación. El esquema fiscal y su integración siguen pendientes del certificador.

## Transacción, detalle e históricos

Se mantiene el bloqueo del turno y aislamiento Serializable. Las lecturas de productos y presentaciones son por lote; inventario, cliente nuevo, venta, clave idempotente, pago y movimiento se confirman juntos. Los errores inesperados salen sin trazas y llevan traceId. Se aplica rate limit por identidad y límite al cuerpo HTTP. Cancelación propagada en registro, edición y abono.

La creación guarda precio, unidades descontadas, costo unitario y ganancia. El detalle consulta el precio guardado. No se rellenan datos históricos usando precios actuales: los registros antiguos con precio/unidades/costo faltantes requieren revisión y no se modifican automáticamente. Edición reutiliza las reglas y el bloqueo para evitar saltarse la validación por otro endpoint. No se implementan anulaciones ni devoluciones en esta corrección.

El abono actualiza monto_pagado, saldo_pendiente y estado dentro de su transacción, y registra EFECTIVO. La clave idempotente añadida cubre la CREACIÓN de la venta, no los abonos independientes; estos no deben reintentarse automáticamente tras un resultado incierto.

## Verificación

Las pruebas de tests/Ventas.Checks usan SQLite temporal y no cargan appsettings ni la cadena de conexión del negocio. Comprueban cálculo, reglas de costo/margen, inventario, pago parcial, idempotencia, reversión y errores seguros. Validar además concurrencia y script en SQL Server de pruebas antes de producción: SQLite no reproduce bloqueos ni deadlocks de SQL Server.

No hay un máximo de detalles al crear o editar una venta; se exige al menos una línea válida. El límite de tamaño HTTP de 128 KiB sigue vigente y no equivale a un número fijo de detalles.
