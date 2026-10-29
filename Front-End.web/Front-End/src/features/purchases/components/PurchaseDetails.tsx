import { PurchasePaymentsHistory } from './PurchasePaymentsHistory'
﻿import { createPortal } from 'react-dom'
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
  const storageKey = 'purchase-payment-' + purchase.id_compra
  const [pending] = useState<{ id: string; payment: string; note: string } | null>(() => {
    try {
      const saved = JSON.parse(sessionStorage.getItem(storageKey) || 'null')
      return saved && typeof saved.id === 'string' && typeof saved.payment === 'string' && typeof saved.note === 'string' ? saved : null
    } catch { return null }
  })
  const operation = useRef(pending)
  const [payment, setPayment] = useState(pending?.payment ?? '')
  const [note, setNote] = useState(pending?.note ?? '')
  const [saving, setSaving] = useState(false)
  const [uncertain, setUncertain] = useState(!!pending)
  const busy = useRef(false)
  const dialogRef = useRef<HTMLDialogElement>(null)
  const paymentsDialog = useRef<HTMLDialogElement>(null)
  const paymentsTrigger = useRef<HTMLButtonElement>(null)
  const [paymentsOpen, setPaymentsOpen] = useState(false)
  useEffect(() => {
    if (!paymentsOpen) return
    const dialog = paymentsDialog.current
    dialog?.showModal()
    return () => { dialog?.close(); paymentsTrigger.current?.focus() }
  }, [paymentsOpen])
  useEffect(() => {
    const dialog = dialogRef.current
    const trigger = document.activeElement instanceof HTMLElement ? document.activeElement : null
    dialog?.showModal()
    const overflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => { dialog?.close(); document.body.style.overflow = overflow; trigger?.focus() }
  }, [])
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
    if (busy.current || (!uncertain && !canPay)) return
    const cents = amountCents(payment)
    if (cents === null || cents <= 0 || (!uncertain && cents > Math.round(purchase.saldo_pendiente * 100))) {
      setError('El abono debe ser mayor a cero y no superar el saldo.'); return
    }
    if (!operation.current) operation.current = { id: crypto.randomUUID(), payment, note }
    try { sessionStorage.setItem(storageKey, JSON.stringify(operation.current)) }
    catch { setError('No se pudo conservar la operación para reintentos. Habilita el almacenamiento del navegador.'); return }
    busy.current = true; setSaving(true); setError('')
    try { await payPurchase(token, purchase.id_compra, cents / 100, note, operation.current.id); sessionStorage.removeItem(storageKey); operation.current = null; onPaid() }
    catch (failure) {
      if (failure instanceof ApiError && failure.status === 401) onUnauthorized()
      else {
        const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500
        const keepOperation = ambiguous || (failure instanceof ApiError && failure.status === 409)
        setUncertain(keepOperation)
        if (!keepOperation) { sessionStorage.removeItem(storageKey); operation.current = null }
        setError(ambiguous ? 'No se pudo confirmar el abono. Reintenta con los mismos datos; no se registrará dos veces.'
          : failure.message)
      }
    } finally { busy.current = false; setSaving(false) }
  }
  return createPortal(<><dialog ref={dialogRef} className="purchases-page purchase-panel purchase-detail-dialog" aria-labelledby="purchase-detail-title" onCancel={event => { event.preventDefault(); if (!busy.current) onClose() }}>
    <div className="purchase-panel-heading purchase-detail-heading"><h2 id="purchase-detail-title">{purchase.nombre_proveedor}</h2>
      <span className="purchase-detail-reference">Compra #{purchase.id_compra}</span>
      <button type="button" className="purchase-detail-close" aria-label="Cerrar detalle" title="Cerrar detalle" disabled={saving} onClick={onClose}>×</button></div>

    <div className="purchase-payment-actions">
      <span className={'purchase-payment-status ' + (purchase.saldo_pendiente > 0 ? 'is-pending' : 'is-paid')}>{purchase.saldo_pendiente > 0 ? 'Pendiente' : 'Cancelada'}</span>
      <button type="button" ref={paymentsTrigger} onClick={() => setPaymentsOpen(true)}>{purchase.saldo_pendiente > 0 ? 'Abonar' : 'Pagos'}</button>
    </div>
    {loading && <p role="status">Cargando detalle…</p>}
    {error && <p role="alert" className="purchase-error">{error}</p>}
    {!loading && !items.length && error && <button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar detalle</button>}
    {!loading && items.length > 0 && <div className="purchase-table-scroll"><table>
      <thead><tr><th>Producto</th><th>Unidades</th><th>Total de línea</th></tr></thead>
      <tbody>{items.map(row => <tr key={row.id_detalle_compra}><td>{row.nombre_producto}</td>
        <td>{row.cantidad}</td><td>{money.format(row.subtotal)}</td></tr>)}</tbody>
    </table></div>}
    {purchase.saldo_pendiente <= 0 && <div className="purchase-detail-balance"><span>Saldo pendiente</span><strong>{money.format(purchase.saldo_pendiente)}</strong></div>}
  </dialog>
  {paymentsOpen && <dialog ref={paymentsDialog} className="purchases-page purchase-panel purchase-detail-dialog purchase-payments-dialog" aria-labelledby="purchase-payments-title"
    onCancel={event => { event.preventDefault(); if (!busy.current) setPaymentsOpen(false) }}>
    <div className="purchase-panel-heading"><h2 id="purchase-payments-title">{purchase.saldo_pendiente > 0 ? 'Abonar' : 'Pagos'} · {purchase.nombre_proveedor}</h2>
      <button type="button" className="purchase-detail-close" aria-label="Cerrar pagos" disabled={saving} onClick={() => setPaymentsOpen(false)}>×</button></div>
    <p>Saldo pendiente: <strong>{money.format(purchase.saldo_pendiente)}</strong></p>
    <PurchasePaymentsHistory token={token} purchaseId={purchase.id_compra} onUnauthorized={onUnauthorized} />
    {error && <p role="alert" className="purchase-error">{error}</p>}
    {!canPay && purchase.saldo_pendiente > 0 && !uncertain && <p>Abre la caja para poder abonar la compra.</p>}
    {(purchase.saldo_pendiente > 0 || uncertain) && <form className="purchase-payment-form" onSubmit={submit}>
      <fieldset disabled={saving || (!canPay && !uncertain)} className="purchase-fields">
        <legend className="purchase-payment-heading">Registrar abono <span className="purchase-payment-balance">Saldo: <strong>{money.format(purchase.saldo_pendiente)}</strong></span></legend>
        <label>Monto<input inputMode="decimal" readOnly={uncertain} placeholder="Ej. 250.00" value={payment} onChange={e => setPayment(e.target.value)} required /></label>
        <label>Observación<input readOnly={uncertain} maxLength={100} placeholder="Ej. Abono en efectivo (opcional)" value={note} onChange={e => setNote(e.target.value)} /></label>
        <button className="purchase-primary" type="submit">{saving ? 'Guardando…' : uncertain ? 'Reintentar abono' : 'Registrar abono'}</button>
      </fieldset>
    </form>}
  </dialog>}</>, document.body)
}
