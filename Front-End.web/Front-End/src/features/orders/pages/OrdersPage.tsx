import { useEffect, useRef, useState } from 'react'
import { SalesPage } from '../../sales/pages/SalesPage'
import { createPortal } from 'react-dom'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { listPendingOrders, getOrderDetail } from '../api/ordersApi'
import type { OrderDetail, OrderPage, OrderSummary } from '../api/ordersApi'
import '../../sales/pages/SalesPage.css'
import '../../sales/pages/SaleListPage.css'
import './OrdersPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
// SQL Server almacena fecha_utc; sus cadenas sin sufijo también deben interpretarse como UTC.
const formatDate = (value: string) => date.format(new Date(/(?:Z|[+-]\d{2}:\d{2})$/i.test(value) ? value : value + 'Z'))

function OrderDialog({ order, token, onClose, onExpired, onPrepare }: { order: OrderSummary; token: string; onClose: () => void; onExpired: () => void; onPrepare: (detail: OrderDetail) => void }) {
  const dialog = useRef<HTMLDialogElement>(null)
  const [detail, setDetail] = useState<OrderDetail | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  useEffect(() => {
    const panel = dialog.current, focus = document.activeElement
    const overflow = document.body.style.overflow
    panel?.showModal(); document.body.style.overflow = 'hidden'
    return () => { panel?.close(); document.body.style.overflow = overflow; if (focus instanceof HTMLElement && focus.isConnected) focus.focus() }
  }, [])
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setError(''); setDetail(null)
    void getOrderDetail(token, order.idPedido, controller.signal).then(v => { if (!controller.signal.aborted) setDetail(v) })
      .catch(failure => {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) onExpired()
        else setError(failure instanceof ApiError && failure.status === 404 ? 'El pedido ya no está disponible.' : failure instanceof Error ? failure.message : 'No se pudo consultar el pedido.')
      }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, order.idPedido, revision, onExpired])
  return createPortal(<dialog ref={dialog} className="sales-page sale-list-page sale-payment-dialog orders-dialog" aria-labelledby="order-title"
    onCancel={event => { event.preventDefault(); onClose() }}>
    <section className="sales-card"><div className="sales-heading"><h2 id="order-title"><span className="sale-payment-client-label">Cliente</span>{order.nombreCliente}</h2>
      <button type="button" onClick={onClose} autoFocus>Cerrar</button></div>
      {loading ? <p role="status">Cargando pedido…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> : detail && <>
        <dl className="sale-payment-summary orders-contact">
          <div><dt>Fecha</dt><dd>{formatDate(detail.fechaUtc)}</dd></div><div><dt>Teléfono</dt><dd>{detail.telefono}</dd></div>
          <div><dt>Zona de entrega</dt><dd>{detail.zonaEntrega}</dd></div><div><dt>Dirección</dt><dd>{detail.direccion}</dd></div>
          {detail.referencias && <div><dt>Referencias</dt><dd>{detail.referencias}</dd></div>}
          {detail.observacion && <div><dt>Observación</dt><dd>{detail.observacion}</dd></div>}
        </dl>
        <div className="sales-table-scroll" role="region" aria-label="Productos del pedido" tabIndex={0}><table>
          <caption>Productos solicitados</caption><thead><tr><th scope="col">Producto</th><th scope="col">Presentación</th><th scope="col">Cantidad</th><th scope="col">Precio</th><th scope="col">Subtotal</th></tr></thead>
          <tbody>{detail.detalles.map(line => <tr key={line.idProductoPresentacion}><td>{line.nombreProducto}</td><td>{line.nombrePresentacion}</td><td>{line.cantidad}</td>
            <td className="sale-list-money">{money.format(line.precioUnitario)}</td><td className="sale-list-money">{money.format(line.subtotal)}</td></tr>)}</tbody>
        </table></div>
        <dl className="sale-payment-summary sale-payment-totals"><div><dt>Total</dt><dd>{money.format(detail.total)}</dd></div>
          <div><dt>Estado</dt><dd><span className="sale-list-status">{detail.estado}</span></dd></div></dl>
        {detail.estado === 'Pendiente' && <button type="button" onClick={() => onPrepare(detail)}>Preparar venta</button>}
      </>}
    </section>
  </dialog>, document.body)
}

export function OrdersPage() {
  const { session, logout } = useAuth()
  const token = session?.token
  const allowed = session?.user.role === 'ADMINISTRADOR'
  const [prepared, setPrepared] = useState<OrderDetail | null>(null)
  const [success, setSuccess] = useState('')
  const [updatedAt, setUpdatedAt] = useState<Date | null>(null)
  const [copyStatus, setCopyStatus] = useState('')
  async function copyCatalogLink() {
    try {
      await navigator.clipboard.writeText(new URL(import.meta.env.BASE_URL + 'catalogo', window.location.origin).href)
      setCopyStatus('Enlace copiado.')
    } catch {
      setCopyStatus('No se pudo copiar el enlace. Abre el catálogo y copia la dirección del navegador.')
    }
  }
  const [page, setPage] = useState(1)
  const [revision, setRevision] = useState(0)
  const [data, setData] = useState<OrderPage | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [selected, setSelected] = useState<OrderSummary | null>(null)
  useEffect(() => {
    if (!token || !allowed) return
    const controller = new AbortController()
    setLoading(true); setError('')
    void listPendingOrders(token, page, controller.signal).then(v => {
      if (controller.signal.aborted) return
      if (v.pedidos.length === 0 && page > 1) setPage(1)
      else { setData(v); setUpdatedAt(new Date()); setCopyStatus('') }
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof Error ? failure.message : 'No se pudieron cargar los pedidos.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, allowed, page, revision, logout])
  if (!allowed || !token) return null
  if (prepared) return <SalesPage key={prepared.idPedido} order={prepared}
    onCancelOrder={() => setPrepared(null)} onOrderConfirmed={() => { setPrepared(null); setSuccess('Venta registrada correctamente.'); setRevision(v => v + 1) }} />
  return <section className="sales-page sale-list-page" aria-label="Pedidos pendientes del catálogo">
    {selected && <OrderDialog key={selected.idPedido} order={selected} token={token} onClose={() => setSelected(null)} onExpired={logout} onPrepare={detail => { setSelected(null); setSuccess(''); setPrepared(detail) }} />}
    {success && <p role="status">{success}</p>}
    <div className="sales-card"><div className="sales-heading"><div><h2>Pendientes de aprobación</h2><p className="sale-list-description">{loading ? 'Consultando pendientes…' : error ? 'No se pudieron consultar los pendientes' : data ? (data.hayMas ? 'Más de ' : '') + ((data.pagina - 1) * data.tamanoPagina + data.pedidos.length) + (data.pagina === 1 && data.pedidos.length === 1 && !data.hayMas ? ' pendiente' : ' pendientes') : '0 pendientes'}</p></div>
      <RefreshButton label="Actualizar pedidos" loading={loading} onClick={() => { setPage(1); setRevision(v => v + 1) }} /></div>
      {loading ? <p role="status">Cargando pedidos…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> : !data?.pedidos.length ?
        <div className="orders-empty-state">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="m4 4-3 10v6h22v-6L20 4Z" /><path d="M1 14h6l2 3h6l2-3h6" />
          </svg>
          <h3>Todo al día</h3>
          <p>No hay pedidos esperando aprobación. Los nuevos pedidos del catálogo aparecerán aquí.</p>
          {updatedAt && <small>Última actualización: <time dateTime={updatedAt.toISOString()}>{updatedAt.toLocaleTimeString('es-GT', { hour: 'numeric', minute: '2-digit' })}</time></small>}
          <button type="button" className="orders-copy-link" onClick={() => void copyCatalogLink()}>Copiar enlace del catálogo</button>
          {copyStatus && <p role="status">{copyStatus}</p>}
        </div> : <div className="sales-table-scroll" role="region" aria-label="Listado de pedidos" tabIndex={0}>
          <table><caption>Selecciona un pedido para consultar sus productos y datos de entrega.</caption><thead><tr><th scope="col">Fecha</th><th scope="col">Cliente</th><th scope="col">Teléfono</th><th scope="col">Zona</th><th scope="col" className="sale-list-money">Total</th><th scope="col">Estado</th></tr></thead>
            <tbody>{data.pedidos.map(order => <tr key={order.idPedido} className="sale-list-row" tabIndex={0} aria-label={'Consultar pedido de ' + order.nombreCliente}
              aria-haspopup="dialog" onClick={() => setSelected(order)} onKeyDown={e => { if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setSelected(order) } }}>
              <td className="sale-list-nowrap">{formatDate(order.fechaUtc)}</td><td>{order.nombreCliente}</td><td>{order.telefono}</td><td>{order.zonaEntrega}</td><td className="sale-list-money">{money.format(order.total)}</td><td><span className="sale-list-status">{order.estado}</span></td>
            </tr>)}</tbody></table>
        </div>}
      {!loading && !error && !!data?.pedidos.length && (page > 1 || data.hayMas) && <nav className="sales-history-pagination" aria-label="Páginas de pedidos"><button type="button" disabled={loading || page === 1} onClick={() => setPage(v => v - 1)}>Anterior</button>
        <span role="status">Página {page}</span><button type="button" disabled={loading || !!error || !data?.hayMas} onClick={() => setPage(v => v + 1)}>Siguiente</button></nav>}
    </div>
  </section>
}
