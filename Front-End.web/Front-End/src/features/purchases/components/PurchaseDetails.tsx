import { useEffect, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { getPurchaseDetails, payPurchase } from '../api/purchasesApi'
import { amountCents } from '../utils/purchaseValidation'
import type { Purchase, PurchaseDetail } from '../types/purchase'
import { useRef } from 'react'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PurchaseDetails({ purchase, token, canPay, onClose, onPaid, onUnauthorized }: {
  purchase: Purchase; token: string; canPay: boolean; onClose: () => void; onPaid: () => void; onUnauthorized: () => void
}) {
  const [items, setItems] = useState<PurchaseDetail[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [payment, setPayment] = useState('')
  const [note, setNote] = useState('')
  const [saving, setSaving] = useState(false)
  const [uncertain, setUncertain] = useState(false)
  const busy = useRef(false)
  useEffect(() => {
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError('')
      await getPurchaseDetails(token, purchase.id_compra, controller.signal).then(rows => {
      if (!controller.signal.aborted) setItems(rows)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) onUnauthorized()
      else setError(failure instanceof Error ? failure.message : 'No se pudo cargar el detalle.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    }
    void load()
    return () => controller.abort()
  }, [token, purchase.id_compra, revision, onUnauthorized])
  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current || uncertain || !canPay) return
    const cents = amountCents(payment)
    if (cents === null || cents <= 0 || cents > Math.round(purchase.saldo_pendiente * 100)) {
      setError('El abono debe ser mayor a cero y no superar el saldo.'); return
    }
    busy.current = true; setSaving(true); setError('')
    try { await payPurchase(token, purchase.id_compra, cents / 100, note); onPaid() }
    catch (failure) {
      if (failure instanceof ApiError && failure.status === 401) onUnauthorized()
      else {
        const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500
        setUncertain(ambiguous)
        setError(ambiguous ? 'No se pudo confirmar el abono. Cierra este detalle y actualiza el historial antes de volver a pagar.'
          : failure.message)
      }
    } finally { busy.current = false; setSaving(false) }
  }
  return <section className="purchase-panel" aria-labelledby="purchase-detail-title">
    <div className="purchase-panel-heading"><h2 id="purchase-detail-title">Compra #{purchase.id_compra}</h2>
      <button type="button" disabled={saving} onClick={onClose}>Cerrar detalle</button></div>
    <p>{purchase.nombre_proveedor} · Saldo: {money.format(purchase.saldo_pendiente)}</p>
    {loading && <p role="status">Cargando detalle…</p>}
    {error && <p role="alert" className="purchase-error">{error}</p>}
    {!loading && !items.length && error && <button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar detalle</button>}
    {!loading && items.length > 0 && <div className="purchase-table-scroll"><table>
      <thead><tr><th>Producto</th><th>Unidades</th><th>Total de línea</th></tr></thead>
      <tbody>{items.map(row => <tr key={row.id_detalle_compra}><td>{row.nombre_producto}</td>
        <td>{row.cantidad}</td><td>{money.format(row.subtotal)}</td></tr>)}</tbody>
    </table></div>}
    {purchase.saldo_pendiente > 0 && <form onSubmit={submit}>
      <fieldset disabled={saving || !canPay || uncertain} className="purchase-fields">
        <legend>Registrar abono</legend>
        <label>Monto<input inputMode="decimal" value={payment} onChange={e => setPayment(e.target.value)} required /></label>
        <label>Observación<input maxLength={100} value={note} onChange={e => setNote(e.target.value)} /></label>
        <button className="purchase-primary" type="submit">{saving ? 'Guardando…' : 'Registrar abono'}</button>
      </fieldset>
    </form>}
  </section>
}
