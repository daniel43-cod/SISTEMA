# Ventas de mostrador

La pantalla usa los endpoints existentes de Ventas. El registro envía id_operacion para impedir duplicados y lee el identificador y totales confirmados por el servidor. El catálogo y el historial se consultan paginados; los clientes inactivos no se excluyen.

El backend admite crédito, pago parcial y descuentos según las reglas confirmadas, pero los controles de esta pantalla registran pago completo en efectivo y permiten un descuento en quetzales por línea. La interfaz de abonos sigue pendiente.

Antes de desplegar, revisar y aplicar con autorización BACKEND/docs/sql/ventas-registro-seguro.sql. El script está preparado, no ejecutado. La API nueva necesita ese esquema. Consultar BACKEND/docs/ventas-registro-seguro.md para contratos y límites.

La certificación sigue pendiente. No se generan documentos ni autorizaciones fiscales. Tras una respuesta incierta se conserva la clave de operación; no se reenvía automáticamente. No se realizaron ventas reales durante las pruebas.

## Captura del cliente

Nombre, apellido, NIT, DPI, teléfono, correo y dirección están visibles desde el inicio. Nombre muestra coincidencias del índice local (ID + nombre); las flechas y Enter permiten seleccionar. Al seleccionar se consulta el detalle y se llenan los campos. Si no se selecciona ningún cliente, al registrar la venta se envían id_cliente=0 y clienteNuevo.

Por decisión del usuario, modificar cualquier campo después de una selección conserva los valores pero quita la asociación al ID: al registrar la venta se crea otro cliente, sin actualizar el original. Si una consulta de detalle falla, no se registra la venta hasta corregir la selección o comenzar una captura manual. Las respuestas canceladas o antiguas no sobrescriben los campos.

## Captura por fila

El buscador superior se reemplazó por un buscador de producto + presentación en cada fila. El índice se construye con los objetos del catálogo y conserva el ID real de producto y presentación; la etiqueta concatenada solo se usa para mostrar y filtrar. Escribir Coca Cola permite elegir Coca Cola · Unidad o Coca Cola · Caja. No se deducen IDs desde la etiqueta.

Flechas y Enter seleccionan; la cantidad corresponde a esa presentación. Enter en cantidad pasa a Descuento; Enter en Descuento pasa a la siguiente fila o crea una fila vacía automáticamente. No hay botón para agregar filas. Una presentación ya seleccionada se modifica en su propia fila para no duplicar IDs que el backend rechaza. Las existencias se validan acumuladas entre presentaciones del mismo producto. Una fila escrita sin selección bloquea el envío; una fila vacía no se envía.

El código de barras se consulta con Enter desde la misma fila y muestra las presentaciones del producto encontrado, sin elegir automáticamente un empaque. Las consultas se cancelan si cambia el texto o se abandona la fila. El desplegable flota fuera de la tabla y puede abrir hacia arriba para evitar recortes.

Descuento (Q) inicia en 0.00, admite hasta dos decimales y se resta una sola vez del importe de la línea (cantidad × precio). El total, efectivo y cambio usan el importe neto. La API sigue siendo la autoridad del costo promedio: rechaza un precio final igual o menor al costo de las unidades vendidas. Ese costo no se expone al frontend.
