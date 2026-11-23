# Impresión de ventas en POS de red

La API está alojada en internet. Esta versión prepara el ticket en el backend y utiliza el
diálogo de impresión del navegador en el equipo del negocio. Allí se selecciona una impresora
POS instalada en el sistema operativo mediante su conexión de red. No abre conexiones desde
la API hacia direcciones privadas ni exige publicar la impresora en internet.

## Uso

1. En el historial de ventas, abrir los tres puntos y entrar al detalle de la venta.
2. Pulsar **Imprimir ticket**.
3. Revisar la vista previa, configurada para **80 mm**.
4. Pulsar **Imprimir en POS de red**. Elegir la POS que esté instalada en el equipo.
5. En el controlador y diálogo de impresión, seleccionar el mismo ancho de papel, escala 100 %,
   márgenes mínimos/sin márgenes según el equipo y desactivar los encabezados y pies del navegador.

El ticket tiene ancho fijo de **80 mm**, con margen interno de 3 mm por lado.
No se utiliza el valor anterior de `pos.anchoPapel` ni se almacena información fiscal en localStorage. La longitud del rollo,
corte automático, impresora predeterminada y copias dependen del controlador y del diálogo.
Se utiliza `@page size:auto`: el tamaño físico lo determina la impresora elegida, no el ancho
de la vista previa. Imprimir a PDF puede permitir revisar el contenido antes de tener la POS.

## Datos y estados

- `GET /api/comprobantes/ventas/{idVenta}/ticket-pos` devuelve datos del ticket. Requiere
  Administrador/Vendedor, sesión válida y permiso Vender; no tiene caché y usa el límite de ventas.
- No recibe precios, totales ni datos fiscales desde el frontend. No cambia estados de venta,
  certificación, inventario ni pagos. Consultar el endpoint no imprime nada.
- Sin DTE certificado en el ambiente configurado: se imprime **COMPROBANTE INTERNO - NO ES
  FACTURA FEL**. Incluye los datos actuales de la venta y empresa.
- Con DTE certificado: los datos del emisor, receptor, líneas, fechas e importes se obtienen del
  XML certificado almacenado y se comprueba su correspondencia con el original. Cambiar los
  datos de la empresa, cliente o producto no cambia la reimpresión fiscal. El vendedor es un dato
  interno obtenido de la venta. No se muestra costo ni ganancia del negocio.
- Se imprimen UUID, serie, número DTE, fecha de certificación, nombre/NIT del certificador,
  frases fiscales admitidas y QR de consulta SAT. Los documentos PRUEBAS mantienen la leyenda
  **SIN VALIDEZ FISCAL**. El QR no vuelve fiscal un documento de pruebas.
- El IVA incluido se presenta como información; no se agrega nuevamente al total.
- La lista de textos fiscales admite tipo/escenario 1/1, 1/2, 2/1 y 3/1. Un escenario sin texto
  admitido bloquea ese ticket y pide utilizar el PDF certificado de INFILE; no se omite la frase.
- No hay certificación automática al imprimir, ni reintentos de impresión. Abrir/cerrar el diálogo
  no permite saber si salió papel: no se marca la venta como "impresa".

## Configuración pendiente de la impresora real

Confirmar marca/modelo, ancho, controlador del fabricante, IP de la POS y conectividad desde
el equipo que imprime. Instalarla como impresora de red en ese equipo y realizar una prueba
de legibilidad, QR, márgenes, páginas largas y corte. En teléfonos, la disponibilidad depende
del servicio de impresión y controlador compatible del sistema; no se garantiza por ser PWA.

La **impresión directa sin diálogo por ESC/POS/TCP** no se incluye en esta versión. Con la API
en internet requeriría un conector local u otra integración del fabricante dentro de la red del
negocio. No basta con introducir la IP de la POS en el backend remoto.

## Validación

- `dotnet run --project tests/Fel.Checks/Fel.Checks.csproj`: incluye `TicketPosChecks` con
  comprobante interno, IVA, metadatos, CF/NIT/CUI, frases, XML alterado y avisos PRUEBAS.
- `npm test` en el frontend: incluye `ticketPos.test.ts`, con contrato HTTP, tamaños, QR local,
  validación de URL SAT y escape de contenido HTML.
- `npm run build`: TypeScript, compilación y PWA.

Las pruebas no envían datos a INFILE ni a una impresora física. La prueba de impresión en la
POS real queda pendiente hasta conocer el equipo.

Referencias: [Window.print](https://developer.mozilla.org/en-US/docs/Web/API/Window/print),
[generación local de QR](https://github.com/soldair/node-qrcode) y
[reglas FEL, frases y QR](https://documentacion.digifact.com/gt/documentos/100).
