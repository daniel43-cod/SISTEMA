import { createPortal } from 'react-dom'
import { useEffect, useRef, useState } from 'react'
import { loadSaleDetails } from '../../sales/api/salesApi'
import type { SaleDetail } from '../../sales/api/salesApi'
import type { ClientSale } from '../api/clientsApi'
import { SalePaymentsHistory } from '../../sales/components/SalePaymentsHistory'
import { SaleDetailSurtido } from '../../sales/components/SaleDetailSurtido'
import { WhatsAppIcon, WhatsAppReceiptDialog } from '../../sales/components/WhatsAppReceipt'
import { TicketPosDialog } from '../../sales/components/TicketPosDialog'
import { ApiError } from '../../../shared/api/ApiError'
import '../../sales/pages/SalesPage.css'
import '../../sales/pages/SaleListPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
export function ClientSaleDetails({ sale, clientName, token, fail, onClose }: { sale: ClientSale; clientName: string; token: string; fail: (e: unknown) => string; onClose: () => void }) {
  const dialog = useRef<HTMLDialogElement>(null)
  const heading = useRef<HTMLHeadingElement>(null)
  const [details, setDetails] = useState<SaleDetail[] | null>(null)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [sharing, setSharing] = useState(false)
  const [paymentsOpen, setPaymentsOpen] = useState(false)
  const [printing, setPrinting] = useState(false)
  const [desktop, setDesktop] = useState(() => window.matchMedia('(min-width: 801px)').matches)
  useEffect(() => {
    const media = window.matchMedia('(min-width: 801px)')
    const update = () => setDesktop(media.matches)
    media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [])
  useEffect(() => {
    const previous = document.activeElement
    const overflow = document.body.style.overflow
    const panel = dialog.current
    panel?.showModal()
    document.body.style.overflow = 'hidden'
    heading.current?.focus()
    return () => { panel?.close(); document.body.style.overflow = overflow; if (previous instanceof HTMLElement && previous.isConnected) previous.focus() }
  }, [])
  useEffect(() => {
    const abort = new AbortController()
    setDetails(null); setError('')
    void loadSaleDetails(sale.idVenta, token, abort.signal).then(d => { if (!abort.signal.aborted) setDetails(d) })
      .catch(e => { if (!abort.signal.aborted) setError(fail(e)) })
    return () => abort.abort()
  }, [sale.idVenta, token, fail, revision])
  return createPortal(<><dialog ref={dialog} className={`sales-page sale-list-page sale-payment-dialog client-history-sale-dialog${desktop ? ' sale-detail-desktop-dialog' : ''}`} aria-labelledby="client-history-sale-title" onCancel={onClose}>
    <section className="sales-card sale-payment">
      <header className="sale-detail-header">
        <div className="sale-detail-title-row"><h2 ref={heading} tabIndex={-1} id="client-history-sale-title">{desktop ? `Venta #${sale.idVenta}` : `Cliente: ${clientName}`}
          {desktop && <span className={sale.saldo === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.saldo === 0 ? '✓ Pagada' : sale.estado}</span>}</h2>
          {!desktop && <div className="sale-mobile-detail-tools">
            <button type="button" aria-label="Imprimir" title="Imprimir" aria-haspopup="dialog" onClick={() => setPrinting(true)}><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true"><path d="M6 9V3h12v6M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2M6 14h12v7H6zM18 12h.01" /></svg></button>
            <button type="button" className="sale-mobile-share-icon" aria-label="Compartir por WhatsApp" title="Compartir por WhatsApp" aria-haspopup="dialog" onClick={() => setSharing(true)}><WhatsAppIcon /></button>
            <button type="button" aria-label="Ver pagos" title="Ver pagos" aria-haspopup="dialog" onClick={() => setPaymentsOpen(true)}><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2" /><path d="M3 10h18M7 15h4" /></svg></button>
          </div>}
          <button type="button" className="sale-detail-close" aria-label="Cerrar detalle de venta" title="Cerrar" onClick={onClose}>×</button></div>
        <p className="sale-detail-metadata">{desktop && <span>Cliente: <strong>{clientName}</strong></span>}
          <span>Atendió: <strong>{sale.responsable}</strong></span><span>{date.format(new Date(sale.fecha))}</span>
          {!desktop && <span className="client-sale-status-total"><span className={sale.saldo === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.saldo === 0 ? 'Pagada' : sale.estado}</span><strong aria-label={`Total ${money.format(sale.total)}`}>{money.format(sale.total)}</strong></span>}</p>
      </header>
      <div className="sale-detail-layout"><div className="sale-detail-main">
        <div className="sale-detail-products-heading"><h3 className="sale-detail-products-title">Productos</h3></div>
        {error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar detalle</button></div> : details ?
          <div className="sales-table-scroll sale-detail-products"><table><caption className="sr-only">Productos de la venta</caption>
            <thead><tr><th>Producto</th><th>Cant.</th><th>Precio</th><th>Desc.</th><th>Subtotal</th></tr></thead>
            <tbody>{details.map((d, i) => <tr key={i}><td data-label="Presentación y producto">{!desktop && d.id_surtido ? <SaleDetailSurtido detail={d} /> : <>
              {desktop ? <><strong>{d.nombre_producto}</strong><small className="sale-detail-presentation">{d.descripcion_resentacion}</small></> : `${d.descripcion_resentacion} · ${d.nombre_producto}`}
              {d.componentes?.map(c => <small key={c.id_producto} className="sale-detail-presentation">{c.nombre}: {c.cantidad} unidades</small>)}</>}</td>
              <td data-label="Cantidad">{d.cantidad}</td><td data-label="Precio unitario">{money.format(d.precio)}</td><td data-label="Descuento">{money.format(d.descuento)}</td>
              <td data-label="Importe">{money.format((Math.round(d.precio * 100) * d.cantidad - Math.round(d.descuento * 100)) / 100)}</td></tr>)}</tbody>
          </table></div> : <p role="status">Cargando detalle…</p>}
        <dl className="sale-receipt-totals"><div className="sale-receipt-grand-total"><dt>Total</dt><dd>{money.format(sale.total)}</dd></div>
          <div><dt>Pagado</dt><dd>{money.format(sale.pagado)}</dd></div>
          <div className={sale.saldo === 0 ? 'sale-detail-settled' : undefined}><dt>Saldo pendiente</dt><dd>{money.format(sale.saldo)}</dd></div></dl>
      </div></div>
      {desktop && <footer className="sale-detail-actions">
        <button type="button" className="wa-send-button" aria-haspopup="dialog" onClick={() => setSharing(true)}><WhatsAppIcon />Compartir</button>
        <button type="button" className="fel-secondary-button" aria-haspopup="dialog" onClick={() => setPaymentsOpen(true)}>Ver pagos</button>
      </footer>}
    </section>
  </dialog>
    {sharing && <WhatsAppReceiptDialog saleId={sale.idVenta} mode="historial" showCertification={false} onClose={() => setSharing(false)} />}
    {paymentsOpen && <ClientPayments saleId={sale.idVenta} clientName={clientName} onClose={() => setPaymentsOpen(false)} />}
    {printing && <TicketPosDialog saleId={sale.idVenta} token={token} onClose={() => setPrinting(false)} onUnauthorized={() => { fail(new ApiError('La sesión ya no es válida.', 401)); setPrinting(false) }} />}
  </>, document.body)
}

function ClientPayments({ saleId, clientName, onClose }: { saleId: number; clientName: string; onClose: () => void }) {
  const dialog = useRef<HTMLDialogElement>(null)
  useEffect(() => {
    const previous = document.activeElement
    const panel = dialog.current
    panel?.showModal()
    return () => { panel?.close(); if (previous instanceof HTMLElement && previous.isConnected) previous.focus() }
  }, [])
  return <dialog ref={dialog} className="sales-page sale-list-page sale-payment-dialog sale-abono-dialog" aria-labelledby="client-payments-title" onCancel={onClose}>
    <section className="sales-card sale-payment">
      <div className="sale-detail-title-row"><h2 id="client-payments-title">{clientName}</h2><button type="button" className="sale-detail-close" aria-label="Cerrar pagos" onClick={onClose}>×</button></div>
      <SalePaymentsHistory saleId={saleId} />
    </section>
  </dialog>
}
