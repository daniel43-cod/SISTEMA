# Caja compartida

Solo existe un turno abierto para todo el sistema. Cualquier administrador activo puede
abrirlo o cerrarlo; las operaciones mantienen el usuario que realmente las realiza.
El login no abre una caja. Vendedores no pueden abrir ni cerrar caja.

## Base de datos

Aplicar `sql/caja-compartida.sql` con la API detenida antes de usar esta funcionalidad.
El script no modifica datos: rechaza varias sesiones abiertas y crea el índice único
filtrado. El mapeo EF por sí solo no aplica este cambio a una base existente.

Los importes de sesión y movimientos se mapean como `decimal(10,2)`.
Las naturalezas admitidas de movimientos son `INGRESO` y `EGRESO` (sin distinguir
mayúsculas ni espacios exteriores). Un movimiento inválido impide cerrar: no se
omite ni se interpreta por su ID. Revisar este catálogo en SQL Server.

## Endpoints

- `POST /api/Caja/abrir`: `id_caja`, `monto_inicial`, `observacion` opcional.
- `GET /api/Caja/actual`: devuelve el turno abierto, o 204 si no existe.
- `POST /api/Caja/cerrar`: **requiere `id_sesion_caja`**, además de `monto_contado`
  y `observacion_cierre` opcional. Usar el ID devuelto por apertura o consulta actual.
  Esto impide que un formulario antiguo cierre un turno posterior.
- `GET /api/Caja/ListarSesionesCaja`: `pagina` y `tamanoPagina` (máximo 100).
- `GET /api/Caja/ListarSesionesCaja/{idCaja}`: mismo límite, filtrado por caja.

Todos requieren administrador. Límite: 60 solicitudes/minuto por usuario, sin cola;
el exceso devuelve 429 y Retry-After. Peticiones de hasta 16 KiB.

## Consistencia

Apertura/cierre y auditoría se confirman en la misma transacción. Las operaciones
de compras, ventas, abonos y gastos bloquean la sesión dentro de su transacción;
el cierre comparte ese bloqueo. Si el cierre gana, la operación se rechaza.
Si la operación gana, el cierre espera y contabiliza su movimiento confirmado.

Saldo esperado = monto inicial + ingresos - egresos registrados como movimientos.
No se suman las ventas además de los movimientos, para evitar contar dos veces.
No se reconstruyen movimientos faltantes del historial anterior.

Pruebas automatizadas con SQLite verifican permisos, apertura duplicada, sesión
compartida, cierre por otro administrador, saldo y auditoría. El índice de NULL único
y la concurrencia/bloqueos se deben verificar adicionalmente con SQL Server.
