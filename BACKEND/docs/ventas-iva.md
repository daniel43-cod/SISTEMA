# IVA incluido en ventas

Al registrar o editar ventas, las líneas gravadas al 12% guardan su impuesto en
`detalle_venta.impuesto` (decimal(18,6)). Se aplica primero el descuento de la línea.
Base = redondear(importe final / 1.12, 6, AwayFromZero).
IVA = importe final - base, conservando seis decimales.
`ventas.impuesto` es la suma de los impuestos de las líneas, sin volver a calcularlo sobre el total.
No se suma otra vez el IVA al precio, saldo, pago ni movimiento de caja.
`subtotal` conserva su significado anterior en el sistema; no se convierte en base sin IVA.
El cálculo existente de ganancia no cambia en este paso.

El generador FEL utiliza la misma función y compara los impuestos persistidos por línea y venta.
Serializa base e IVA hasta con seis decimales; los precios y totales finales siguen con dos.
Si una venta histórica tiene impuesto NULL en el detalle, se calcula únicamente para construir
el XML nuevo, sin actualizar su historial ni sobrescribir el XML de un intento ya enviado.
Los reintentos POR_CONFIRMAR conservan el XML original, incluso si usa dos decimales.
Esta versión solo contempla líneas afectas al 12%, no productos exentos ni impuestos especiales.

Aplicar primero `docs/postgresql.md`. No hace falta otro script para este paso.
