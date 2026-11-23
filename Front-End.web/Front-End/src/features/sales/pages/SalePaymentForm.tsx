import { TicketPosDialog } from '../components/TicketPosDialog'
import { SalePaymentsHistory } from '../components/SalePaymentsHistory'
import { createPortal } from 'react-dom'
import { useCallback, useEffect, useRef, useState } from 'react'
import { CertifySalePanel } from '../components/CertifySalePanel'
import { WhatsAppIcon, WhatsAppReceiptDialog } from '../components/WhatsAppReceipt'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { loadSaleDetails, paySale } from '../api/salesApi'
import { loadFel } from '../api/felApi'
import type { FelDocument } from '../api/felApi'
import type { SaleDetail, SaleHistory } from '../api/salesApi'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function SalePaymentForm({ sale, onClose, onPaid, onSaving }: { sale: SaleHistory; onClose: () => void; onPaid: () => void; onSaving: (value: boolean) => void }) {
  const { session, logout } = useAuth()
  const token = session?.token
  const dialog = useRef<HTMLDialogElement>(null)
  const paymentDialog = useRef<HTMLDialogElement>(null)
  const certificationDialog = useRef<HTMLDialogElement>(null)
  const paymentButton = useRef<HTMLButtonElement>(null)
  const paymentHeading = useRef<HTMLHeadingElement>(null)
  const paymentAmount = useRef<HTMLInputElement>(null)
  const [paymentOpen, setPaymentOpen] = useState(false)
  const [sharing, setSharing] = useState(false)
  const [printing, setPrinting] = useState(false)
  const [certificationOpen, setCertificationOpen] = useState(false)
  const [certifying, setCertifying] = useState(false)
  const [desktopView, setDesktopView] = useState(() => window.matchMedia('(min-width: 801px)').matches)
  useEffect(() => {
    const media = window.matchMedia('(min-width: 801px)')
    const update = () => { if (!certifying) setDesktopView(media.matches) }
    update()
    media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [certifying])
  const [felDocument, setFelDocument] = useState<FelDocument | null | undefined>(undefined)
  const [felLoading, setFelLoading] = useState(true)
  const certificationBusy = useCallback((value: boolean) => { setCertifying(value); onSaving(value) }, [onSaving])
  const certificationUnauthorized = useCallback(() => logout('La sesión ya no es válida.'), [logout])
  const heading = useRef<HTMLHeadingElement>(null)
  const paymentStorageKey = 'sale-payment-' + session?.user.id + '-' + sale.id_ventas
  const [pending] = useState<{ id: string; amount: number; note: string } | null>(() => {
    try {
      const saved = JSON.parse(sessionStorage.getItem(paymentStorageKey) || 'null')
      return saved && typeof saved.id === 'string' && /^[0-9a-f-]{36}$/i.test(saved.id) &&
        typeof saved.amount === 'number' && Number.isFinite(saved.amount) && saved.amount > 0 && typeof saved.note === 'string' ? saved : null
    } catch { return null }
  })
  const pendingPayment = useRef(pending)
  const busy = useRef(false)
  const alive = useRef(true)
  const [details, setDetails] = useState<SaleDetail[]>([])
  const [loading, setLoading] = useState(true)
  const [detailError, setDetailError] = useState('')
  const [revision, setRevision] = useState(0)
  const [amount, setAmount] = useState(pending ? String(pending.amount) : '')
  const [note, setNote] = useState(pending?.note ?? '')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [uncertain, setUncertain] = useState(!!pending)
  useEffect(() => {
    alive.current = true
    const panel = dialog.current
    const previousOverflow = document.body.style.overflow
    const previousFocus = document.activeElement
    panel?.showModal()
    document.body.style.overflow = 'hidden'
    heading.current?.focus()
    return () => {
      alive.current = false
      panel?.close()
      document.body.style.overflow = previousOverflow
      if (previousFocus instanceof HTMLElement && previousFocus.isConnected) previousFocus.focus()
    }
  }, [])
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    setLoading(true); setDetailError('')
    void loadSaleDetails(sale.id_ventas, token, controller.signal).then(result => {
      if (!controller.signal.aborted) setDetails(result)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setDetailError(failure instanceof Error ? failure.message : 'No se pudo cargar el detalle.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [sale.id_ventas, token, logout, revision])

  useEffect(() => {
    if (!paymentOpen) return
    const panel = paymentDialog.current
    panel?.showModal(); if (sale.saldo_pendiente > 0) paymentAmount.current?.focus(); else paymentHeading.current?.focus()
    return () => { panel?.close(); paymentButton.current?.focus() }
  }, [paymentOpen, sale.saldo_pendiente])

  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    setFelLoading(true); setFelDocument(undefined)
    void loadFel(sale.id_ventas, token, controller.signal).then(document => {
      if (!controller.signal.aborted) setFelDocument(document)
    }).catch(failure => {
      if (!controller.signal.aborted && failure instanceof ApiError && failure.status === 401) certificationUnauthorized()
      // Estado desconocido: ofrecer consulta, nunca asumir que debe volver a certificarse.
    }).finally(() => { if (!controller.signal.aborted) setFelLoading(false) })
    return () => controller.abort()
  }, [sale.id_ventas, token, certificationUnauthorized])

  useEffect(() => {
    if (!certificationOpen) return
    const panel = certificationDialog.current
    const previousFocus = document.activeElement
    panel?.showModal()
    return () => {
      panel?.close()
      if (previousFocus instanceof HTMLElement && previousFocus.isConnected) previousFocus.focus()
    }
  }, [certificationOpen])

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || busy.current) return
    setError('')
    const input = amount.trim().replace(',', '.')
    const value = pendingPayment.current?.amount ?? Number(input)
    if (!pendingPayment.current && (!/^\d+(\.\d{1,2})?$/.test(input) || value <= 0 || Math.round(value * 100) > Math.round(sale.saldo_pendiente * 100))) {
      setError('Ingresa un monto mayor a cero, con hasta dos decimales y que no supere el saldo.'); return
    }
    pendingPayment.current ??= { id: crypto.randomUUID(), amount: value, note: note.trim() }
    try { sessionStorage.setItem(paymentStorageKey, JSON.stringify(pendingPayment.current)) }
    catch { setError("No se pudo conservar la operación. Habilita el almacenamiento del navegador antes de abonar."); return }
    busy.current = true; setSaving(true); onSaving(true)
    try {
      await paySale(sale.id_ventas, pendingPayment.current.amount, pendingPayment.current.note, token, pendingPayment.current.id)
      sessionStorage.removeItem(paymentStorageKey); pendingPayment.current = null; setUncertain(false)
      if (alive.current) onPaid()
    } catch (failure) {
      if (!alive.current) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else {
        const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status === 200 || failure.status >= 500
        if (!ambiguous && !(failure instanceof ApiError && failure.status === 409)) { sessionStorage.removeItem(paymentStorageKey); pendingPayment.current = null }
        setUncertain(ambiguous || (failure instanceof ApiError && failure.status === 409))
        setError(ambiguous ? 'No se pudo confirmar el abono. Reintenta la misma operación: no se registrará un segundo pago.' : failure instanceof Error ? failure.message : 'No se pudo registrar el abono.')
      }
    } finally { busy.current = false; onSaving(false); if (alive.current) setSaving(false) }
  }
  const invoiceAction = felDocument?.estado === 'CERTIFICADA' && felDocument.pdf ?
    <a className="fel-secondary-button" href={felDocument.pdf} target="_blank" rel="noopener noreferrer">Ver factura</a> :
    <button type="button" className="fel-secondary-button" disabled={saving || certifying || felLoading} aria-haspopup="dialog" aria-expanded={certificationOpen} onClick={() => setCertificationOpen(true)}>
      {felLoading ? 'Consultando factura…' : felDocument?.estado === 'CERTIFICADA' ? 'Ver factura' : certificationOpen ? 'Certificar factura' : felDocument?.estado === 'EN_PROCESO' || felDocument?.estado === 'POR_CONFIRMAR' || felDocument === undefined ? 'Consultar factura' : 'Certificar factura'}
    </button>
  const certificationPanel = <CertifySalePanel saleId={sale.id_ventas} token={token ?? ''} active={true}
    onToggle={() => setCertificationOpen(false)} disabled={saving} onBusy={certificationBusy} onUnauthorized={certificationUnauthorized} onDocumentChange={setFelDocument} />
  return createPortal(<><dialog ref={dialog} className={`sales-page sale-list-page sale-payment-dialog${desktopView ? ' sale-detail-desktop-dialog' : ''}`} aria-labelledby="sale-payment-title"
    onCancel={event => { event.preventDefault(); if (!busy.current && !certifying) onClose() }}>
    <section className="sales-card sale-payment">
      <header className="sale-detail-header">
        <div className="sale-detail-title-row">
          <h2 id="sale-payment-title" ref={heading} tabIndex={-1}>{desktopView ? <>Venta #{sale.id_ventas} <span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.saldo_pendiente === 0 ? '✓ Pagada' : sale.estado}</span></> : 'Cliente: ' + sale.cliente}</h2>
          {!desktopView && <div className="sale-mobile-detail-tools">
            <button type="button" disabled={saving || certifying} onClick={() => setPrinting(true)} aria-haspopup="dialog" aria-label="Imprimir ticket" title="Imprimir ticket"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true"><path d="M6 9V3h12v6M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><path d="M6 14h12v7H6zM18 12h.01"/></svg></button>
            <button type="button" className="sale-mobile-share-icon" disabled={saving || certifying} onClick={() => setSharing(true)} aria-haspopup="dialog" aria-label="Compartir por WhatsApp" title="Compartir por WhatsApp"><WhatsAppIcon /></button>
          </div>}
          <button type="button" className="sale-detail-close" disabled={saving || certifying} onClick={onClose} aria-label="Cerrar detalle de venta" title="Cerrar">×</button>
        </div>
        <p className="sale-detail-metadata">
          {desktopView && <span>Cliente: <strong>{sale.cliente}</strong></span>}
          <span>{desktopView ? 'Atendió: ' : 'Usuario: '}<strong>{sale.usuario}</strong></span>
          <span>{Number.isNaN(Date.parse(sale.fecha_venta)) ? 'Fecha no disponible' : new Intl.DateTimeFormat('es-GT', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(new Date(sale.fecha_venta)) + ' · ' + new Intl.DateTimeFormat('es-GT', { hour: 'numeric', minute: '2-digit', hour12: true }).format(new Date(sale.fecha_venta))}</span>
          {!desktopView && <span className="sale-detail-payment-action"><span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.saldo_pendiente === 0 ? 'Pagada' : sale.estado}</span>
            <button ref={paymentButton} type="button" disabled={saving || certifying} onClick={() => setPaymentOpen(true)} aria-haspopup="dialog">{sale.saldo_pendiente > 0 ? 'Abonar' : 'Pagos'}</button>
            {invoiceAction}
          </span>}
        </p>
      </header>
      <div className={`sale-detail-layout${desktopView && felDocument?.estado === 'CERTIFICADA' && !certifying ? ' is-certified' : ''}`}>
        <div className="sale-detail-main">
          {desktopView && <div className="sale-detail-products-heading"><h3 className="sale-detail-products-title">Productos</h3><button ref={paymentButton} type="button" className="fel-secondary-button" disabled={saving || certifying} onClick={() => setPaymentOpen(true)} aria-haspopup="dialog">Pagos</button></div>}
          {loading ? <p role="status">Cargando detalle…</p> : detailError ? <div role="alert"><p>{detailError}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar detalle</button></div> :
            <div className="sales-table-scroll sale-detail-products"><table><caption className="sr-only">Productos de la venta</caption>
              <thead><tr><th scope="col">{desktopView ? 'Producto' : 'Presentación y producto'}</th><th scope="col">{desktopView ? 'Cant.' : 'Cantidad'}</th><th scope="col">Precio</th><th scope="col">{desktopView ? 'Desc.' : 'Descuento'}</th><th scope="col">Subtotal</th></tr></thead>
              <tbody>{details.map((detail, index) => <tr key={index}>
                <td data-label="Presentación y producto">{desktopView ? <><strong>{detail.nombre_producto}</strong><small className="sale-detail-presentation">{detail.descripcion_resentacion}</small></> : [detail.descripcion_resentacion, detail.nombre_producto].filter(Boolean).join(' · ')}</td>
                <td data-label="Cantidad">{detail.cantidad}</td><td data-label="Precio unitario">{money.format(detail.precio)}</td><td data-label="Descuento">{money.format(detail.descuento)}</td><td data-label="Importe">{money.format((Math.round(detail.precio * 100) * detail.cantidad - Math.round(detail.descuento * 100)) / 100)}</td>
              </tr>)}</tbody>
            </table></div>}

          <dl className="sale-receipt-totals">
            <div><dt>Subtotal</dt><dd>{money.format(sale.subtotal)}</dd></div>
            <div><dt>Descuento</dt><dd>− {money.format(sale.descuento ?? 0)}</dd></div>
            <div className="sale-receipt-grand-total"><dt>Total</dt><dd>{money.format(sale.total)}</dd></div>
            <div className={sale.saldo_pendiente === 0 ? 'sale-detail-settled' : undefined}><dt>Saldo pendiente</dt><dd>{money.format(sale.saldo_pendiente)}</dd></div>
            {!desktopView && session?.user.role === 'ADMINISTRADOR' && <div className="sale-receipt-profit"><dt>Ganancia</dt><dd>{sale.ganancia_total === undefined ? 'No disponible' : money.format(sale.ganancia_total)}</dd></div>}
          </dl>
          {desktopView && session?.user.role === 'ADMINISTRADOR' && <div className="sale-detail-profit-card"><div><strong>Ganancia</strong><small>Solo administradores</small></div><strong>{sale.ganancia_total === undefined ? 'No disponible' : money.format(sale.ganancia_total)}</strong></div>}
        </div>

      </div>
      {desktopView && <footer className="sale-detail-actions">
        <button type="button" className="fel-secondary-button" disabled={saving || certifying} onClick={() => setPrinting(true)} aria-haspopup="dialog">Imprimir ticket</button>
        <button type="button" className="wa-send-button" disabled={saving || certifying} onClick={() => setSharing(true)} aria-haspopup="dialog"><WhatsAppIcon />Compartir por WhatsApp</button>
        {invoiceAction}

      </footer>}

    </section>
  </dialog>
  {certificationOpen && <dialog ref={certificationDialog} className="wa-receipt-dialog sale-certification-dialog" aria-labelledby="sale-certification-title"
    onCancel={event => { event.preventDefault(); if (!certifying) setCertificationOpen(false) }}>
    <header className="sale-certification-dialog-header"><h2 id="sale-certification-title">Factura · Venta #{sale.id_ventas}</h2>
      <button type="button" disabled={certifying} aria-label="Cerrar certificación" onClick={() => setCertificationOpen(false)}>×</button>
    </header>
    {certificationPanel}
  </dialog>}
  {printing && <TicketPosDialog saleId={sale.id_ventas} token={token ?? ''} onClose={() => setPrinting(false)} onUnauthorized={certificationUnauthorized} />}
  {sharing && <WhatsAppReceiptDialog saleId={sale.id_ventas} mode="historial" showCertification={false} onClose={() => setSharing(false)} />}
  {paymentOpen && <dialog ref={paymentDialog} className="sales-page sale-list-page sale-payment-dialog sale-abono-dialog"
    aria-labelledby="sale-abono-title" onCancel={event => { event.preventDefault(); if (!busy.current) setPaymentOpen(false) }}>
    <section className="sales-card sale-payment">
      <div className="sale-detail-title-row"><h2 id="sale-abono-title" ref={paymentHeading} tabIndex={-1}>{sale.saldo_pendiente > 0 ? 'Abonar' : 'Pagos'} · {sale.cliente}</h2>
        <button type="button" className="sale-detail-close" disabled={saving} onClick={() => setPaymentOpen(false)} aria-label="Cerrar pagos">×</button>
      </div>
      <p>Saldo pendiente: <strong>{money.format(sale.saldo_pendiente)}</strong></p>
    <SalePaymentsHistory saleId={sale.id_ventas} />
    {(sale.saldo_pendiente > 0 || uncertain) && <form onSubmit={submit}>
      <fieldset className="sales-fields" disabled={saving || uncertain || loading || !!detailError}>
        <legend>Registrar abono en efectivo</legend><div className="sale-payment-fields">
          <label>Monto (Q)<input ref={paymentAmount} inputMode="decimal" value={amount} onChange={event => setAmount(event.target.value)} required maxLength={12} placeholder="0.00" /></label>
          <label>Observación<input value={note} onChange={event => setNote(event.target.value)} maxLength={100} /></label>
        </div><button type="submit">{saving ? 'Registrando abono…' : 'Registrar abono'}</button>
      </fieldset>
      {error && <p role="alert">{error}</p>}
      {uncertain && <button type="submit" disabled={saving}>{saving ? "Consultando abono…" : "Reintentar el mismo abono"}</button>}
    </form>}
    </section>
  </dialog>}
  </>, document.body)
}
