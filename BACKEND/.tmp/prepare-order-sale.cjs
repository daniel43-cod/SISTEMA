const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file, a, b) { const p = path.join(root, file); const s = fs.readFileSync(p, 'utf8'); if (!s.includes(a)) throw Error(file + ': ' + a); fs.writeFileSync(p, s.replace(a,b)); }
const api = 'src/features/orders/api/ordersApi.ts';
fs.appendFileSync(path.join(root, api), `
export async function confirmOrderSale(orderId: number, body: unknown, token: string) {
  const v = await requestJson('/pedidos-catalogo/' + orderId + '/confirmar-venta', { method: 'POST', body, token, validationMessages: true })
  if (!record(v) || !id(v.idVenta) || !amount(v.total) || !amount(v.saldoPendiente))
    throw new ApiError('No se pudo confirmar la respuesta de la venta.', 200)
}
`);
const sales = 'src/features/sales/pages/SalesPage.tsx';
edit(sales, "import { createOperationId }", "import type { OrderDetail } from '../../orders/api/ordersApi'\nimport { confirmOrderSale } from '../../orders/api/ordersApi'\nimport { createOperationId }");
edit(sales, 'export function SalesPage() {', `export function SalesPage({ order, onCancelOrder, onOrderConfirmed }: {
  order?: OrderDetail; onCancelOrder?: () => void; onOrderConfirmed?: () => void
} = {}) {
  const seeded = useRef(false)`);
edit(sales, "const [name, setName] = useState('')", "const [name, setName] = useState(order?.nombreCliente ?? '')");
edit(sales, "const [address, setAddress] = useState('')", "const [address, setAddress] = useState(order?.direccion ?? '')");
edit(sales, "const [phone, setPhone] = useState('')", "const [phone, setPhone] = useState(order?.telefono ?? '')");
edit(sales, 'loadSaleCustomers(token, controller.signal)', 'order ? Promise.resolve([] as SaleCustomer[]) : loadSaleCustomers(token, controller.signal)');
edit(sales, 'if (!controller.signal.aborted) { setProducts(catalog); setCustomers(clients) }', `if (!controller.signal.aborted) {
          setProducts(catalog); setCustomers(clients)
          if (order) {
            const makeRow = (line: OrderDetail['detalles'][number], index: number): SaleDraft => {
              const product = catalog.find(p => p.presentaciones.some(pr => pr.id_producto_presentacion === line.idProductoPresentacion))
              const presentation = product?.presentaciones.find(pr => pr.id_producto_presentacion === line.idProductoPresentacion)
              return { key: index, text: line.nombreProducto + ' · ' + line.nombrePresentacion,
                selection: product && presentation ? { product, presentation, quantity: String(line.cantidad), discount: '0.00' } : null }
            }
            if (!seeded.current) {
              setRows(order.detalles.map(makeRow)); nextRowKey.current = order.detalles.length; seeded.current = true
            } else {
              setRows(previous => previous.map(row => {
                if (!row.selection) return row
                const product = catalog.find(p => p.presentaciones.some(pr => pr.id_producto_presentacion === row.selection!.presentation.id_producto_presentacion))
                const presentation = product?.presentaciones.find(pr => pr.id_producto_presentacion === row.selection!.presentation.id_producto_presentacion)
                return { ...row, selection: product && presentation ? { ...row.selection, product, presentation } : null }
              }))
            }
          }
        }`);
edit(sales, '}, [token, revision, logout])', '}, [token, revision, logout, order])');
edit(sales, "if (newCustomer ? !name.trim()", "if (!order && (newCustomer ? !name.trim()");
edit(sales, "String(c.id_cliente) === customerId)) throw", "String(c.id_cliente) === customerId))) throw");
edit(sales, '      await createSale({', `      if (order) {
        await confirmOrderSale(order.idPedido, { idOperacion: operationId.current, totalEsperado: total / 100,
          pagoInicial: Math.min(receivedCents!, total) / 100,
          detalles: lines.map(l => ({ idProductoPresentacion: l.presentation.id_producto_presentacion, cantidad: Number(l.quantity), descuento: cashCents(l.discount ?? '0.00')! / 100 })) }, token)
        onOrderConfirmed?.(); return
      }
      await createSale({`);
edit(sales, '  const customerFields = (', `  const customerFields = order ? (
    <fieldset id={customerFieldsId} className="sales-page sales-customer sales-customer-header" disabled>
      <label>Cliente<input value={order.nombreCliente} readOnly /></label>
      <label>Teléfono<input value={order.telefono} readOnly /></label>
      <label>Zona de entrega<input value={order.zonaEntrega} readOnly /></label>
      <label>Dirección<input value={order.direccion} readOnly /></label>
      {order.referencias && <label>Referencias<input value={order.referencias} readOnly /></label>}
      {order.observacion && <label>Observación del cliente<input value={order.observacion} readOnly /></label>}
    </fieldset>
  ) : (`);
edit(sales, '<h2 id="sale-title">Nueva venta</h2>', `<h2 id="sale-title">{order ? 'Preparar venta del pedido' : 'Nueva venta'}</h2>
        {order && <button type="button" disabled={pending} onClick={onCancelOrder}>Volver a pedidos</button>}`);
edit(sales, '      {loading && <p role="status">', `      {order && <p>Revisa cantidades y precios actuales; puedes aplicar descuentos. El pedido original se conserva hasta confirmar la venta.</p>}
      {loading && <p role="status">`);
edit(sales, "{pending ? 'Registrando…' : 'Registrar venta'}", "{pending ? 'Registrando…' : order ? 'Confirmar y registrar venta' : 'Registrar venta'}");
const orders = 'src/features/orders/pages/OrdersPage.tsx';
edit(orders, "import { createPortal }", "import { SalesPage } from '../../sales/pages/SalesPage'\nimport { createPortal }");
edit(orders, 'function OrderDialog({ order, token, onClose, onExpired }: { order: OrderSummary; token: string; onClose: () => void; onExpired: () => void })', 'function OrderDialog({ order, token, onClose, onExpired, onPrepare }: { order: OrderSummary; token: string; onClose: () => void; onExpired: () => void; onPrepare: (detail: OrderDetail) => void })');
edit(orders, '      </>}', `        {detail.estado === 'Pendiente' && <button type="button" onClick={() => onPrepare(detail)}>Preparar venta</button>}
      </>}`);
edit(orders, '  const [page, setPage]', "  const [prepared, setPrepared] = useState<OrderDetail | null>(null)\n  const [success, setSuccess] = useState('')\n  const [page, setPage]");
edit(orders, '  return <section className="sales-page', `  if (prepared) return <SalesPage key={prepared.idPedido} order={prepared}
    onCancelOrder={() => setPrepared(null)} onOrderConfirmed={() => { setPrepared(null); setSuccess('Venta registrada correctamente.'); setRevision(v => v + 1) }} />
  return <section className="sales-page`);
edit(orders, 'onExpired={logout} />', "onExpired={logout} onPrepare={detail => { setSelected(null); setSuccess(''); setPrepared(detail) }} />");
edit(orders, '    <div className="sales-card"><div className="sales-heading">', '    {success && <p role="status">{success}</p>}\n    <div className="sales-card"><div className="sales-heading">');
