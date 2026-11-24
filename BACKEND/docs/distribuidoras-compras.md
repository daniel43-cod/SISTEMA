# Distribuidoras al registrar compras

El campo Distribuidora permite elegir una sugerencia existente o escribir un nombre nuevo. La distribuidora se guarda al confirmar la compra, nunca al escribir o avanzar al carrito. No se requieren columnas ni migraciones nuevas.

`POST /api/Compra/crear` admite una de estas dos alternativas, junto a los detalles y el pago habituales:

- `id_proveedor`: ID positivo de una distribuidora existente.
- `nombre_proveedor`: nombre de hasta 100 caracteres. Si coincide con una existente, ignorando mayúsculas y espacios exteriores, se reutiliza su ID; en caso contrario se crea con teléfono vacío y sin datos opcionales de contacto.

No se aceptan ambos campos ni la ausencia de los dos. Seleccionar un ID no modifica los datos del proveedor. Si ya hay duplicados históricos por nombre, se reutiliza el ID menor; este flujo no elimina datos previos.

La resolución y el alta pertenecen a la misma transacción serializable que la compra, el inventario y el pago. Un fallo revierte también la distribuidora nueva. Las compras concurrentes pueden recibir el conflicto 409 existente; al reintentar se consulta de nuevo el proveedor. No hay reintento automático de la petición desde el frontend.

Validación: `tests/CompraProveedor.Checks` usa SQLite temporal para comprobar altas, selección, coincidencias, entradas inválidas, caja cerrada y reversión. `tests/Postgres.Checks` incluye coincidencias y compras simultáneas sobre PostgreSQL cuando se habilita su conexión temporal aislada. El frontend prueba el contrato de ambas alternativas en `tests/purchases.test.ts`.
