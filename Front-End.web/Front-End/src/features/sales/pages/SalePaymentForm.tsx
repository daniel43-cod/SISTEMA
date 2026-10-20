import { createPortal } from 'react-dom'
import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { loadSaleDetails, paySale } from '../api/salesApi'
import type { SaleDetail, SaleHistory } from '../api/salesApi'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function SalePaymentForm({ sale, onClose, onPaid, onSaving }: { sale: SaleHistory; onClose: () => void; onPaid: () => void; onSaving: (value: boolean) => void }) {
  const { session, logout } = useAuth()
  const token = session?.token
  const dialog = useRef<HTMLDialogElement>(null)
  const heading = useRef<HTMLHeadingElement>(null)
  const busy = useRef(false)
  const alive = useRef(true)
  const [details, setDetails] = useState<SaleDetail[]>([])
  const [loading, setLoading] = useState(true)
  const [detailError, setDetailError] = useState('')
  const [revision, setRevision] = useState(0)
  const [amount, setAmount] = useState('')
  const [note, setNote] = useState('')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [uncertain, setUncertain] = useState(false)
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

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || busy.current || uncertain) return
    setError('')
    const input = amount.trim().replace(',', '.')
    const value = Number(input)
    if (!/^\d+(\.\d{1,2})?$/.test(input) || value <= 0 || Math.round(value * 100) > Math.round(sale.saldo_pendiente * 100)) {
      setError('Ingresa un monto mayor a cero, con hasta dos decimales y que no supere el saldo.'); return
    }
    busy.current = true; setSaving(true); onSaving(true)
    try {
      await paySale(sale.id_ventas, value, note.trim(), token)
      if (alive.current) onPaid()
    } catch (failure) {
      if (!alive.current) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else {
        const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status === 200 || failure.status >= 500
        setUncertain(ambiguous)
        setError(ambiguous ? 'No se pudo confirmar el abono. Actualiza el listado y verifica el saldo antes de volver a abonar.' : failure instanceof Error ? failure.message : 'No se pudo registrar el abono.')
      }
    } finally { busy.current = false; onSaving(false); if (alive.current) setSaving(false) }
  }
  return createPortal(<dialog ref={dialog} className="sales-page sale-list-page sale-payment-dialog" aria-labelledby="sale-payment-title"
    onCancel={event => { event.preventDefault(); if (!busy.current) onClose() }}>
    <section className="sales-card sale-payment">
    <div className="sales-heading"><h2 id="sale-payment-title" ref={heading} tabIndex={-1}><span className="sale-payment-client-label">Cliente</span>{sale.cliente}</h2>
      <button type="button" disabled={saving} onClick={onClose}>Cerrar</button></div>
    {loading ? <p role="status">Cargando detalle…</p> : detailError ? <div role="alert"><p>{detailError}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar detalle</button></div> :
      <div className="sales-table-scroll"><table><caption>Productos de la venta</caption><thead><tr><th scope="col">Producto</th><th scope="col">Presentación</th><th scope="col">Cantidad</th><th scope="col">Precio</th><th scope="col">Descuento</th><th scope="col">Subtotal</th></tr></thead>
        <tbody>{details.map((detail, index) => <tr key={index}><td>{detail.nombre_producto}</td><td>{detail.descripcion_resentacion}</td><td>{detail.cantidad}</td>
          <td>{money.format(detail.precio)}</td><td>{money.format(detail.descuento)}</td><td>{money.format((Math.round(detail.precio * 100) * detail.cantidad - Math.round(detail.descuento * 100)) / 100)}</td></tr>)}</tbody></table></div>}
    <dl className="sale-payment-summary sale-payment-totals">
      <div><dt>Total</dt><dd>{money.format(sale.total)}</dd></div>
      {session?.user.role === 'ADMINISTRADOR' && <div><dt>Ganancia</dt><dd>{sale.ganancia_total === undefined ? 'No disponible' : money.format(sale.ganancia_total)}</dd></div>}
      <div><dt>Saldo pendiente</dt><dd>{money.format(sale.saldo_pendiente)}</dd></div>
      <div><dt>Estado</dt><dd><span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.estado}</span></dd></div>
    </dl>
    {sale.saldo_pendiente > 0 ? <form onSubmit={submit}>
      <fieldset className="sales-fields" disabled={saving || uncertain || loading || !!detailError}>
        <legend>Registrar abono en efectivo</legend><div className="sale-payment-fields">
          <label>Monto (Q)<input inputMode="decimal" value={amount} onChange={event => setAmount(event.target.value)} required maxLength={12} placeholder="0.00" /></label>
          <label>Observación<input value={note} onChange={event => setNote(event.target.value)} maxLength={100} /></label>
        </div><button type="submit">{saving ? 'Registrando abono…' : 'Registrar abono'}</button>
      </fieldset>
      {error && <p role="alert">{error}</p>}
      {uncertain && <button type="button" onClick={onClose}>Cerrar y actualizar listado</button>}
    </form> : <p role="status">Esta venta está pagada; no admite nuevos abonos.</p>}
    </section>
  </dialog>, document.body)
}
