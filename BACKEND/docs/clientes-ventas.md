# Clientes en Ventas

GET /api/Cliente/listar devuelve únicamente id_cliente y nombre (nombre completo para la búsqueda local). No excluye clientes inactivos ni impone un máximo de clientes. El frontend conserva un índice de esos dos campos en el estado de la pantalla; no usa localStorage ni guarda fichas completas de todos los clientes.

GET /api/Cliente/{id} consulta el detalle solo al seleccionar: id_cliente, nombre, apellido, NIT, DPI, teléfono, correo y dirección. Los campos se llenan automáticamente y son de lectura en Ventas para clientes existentes. Para registrar una venta existente se envía su ID; los datos no se modifican desde esta pantalla. La creación de cliente nuevo conserva su flujo.

Las dos consultas requieren ADMINISTRADOR o VENDEDOR, usan proyecciones DTO, AsNoTracking y CancellationToken. Los errores internos se registran en el servidor y no se devuelven al navegador. Un cliente inexistente devuelve 404.

Se eliminan el método duplicado ListarCliente, el endpoint /Cliente/buscar y ClienteBuscarDto, ya sin consumidores en el proyecto. La búsqueda por nombre se realiza en memoria. Si existen aplicaciones externas que llamaban a /Cliente/buscar, deberán migrar a este contrato.

El selector cancela la consulta previa al escribir, seleccionar otra persona o cambiar a cliente nuevo; ignora respuestas antiguas y no habilita el registro mientras falten los datos seleccionados. El índice en memoria contiene solo ID y nombre; únicamente los campos del cliente seleccionado permanecen en el formulario.

No requiere cambios SQL ni ejecuta ventas para probar el listado.
