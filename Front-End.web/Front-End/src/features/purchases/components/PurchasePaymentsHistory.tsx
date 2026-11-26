import type { ReactNode } from 'react'
import { useEffect, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { getPurchasePayments } from '../api/purchasesApi'
import type { PurchasePayment } from '../api/purchasesApi'
const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PurchasePaymentsHistory({ token, purchaseId, onUnauthorized, onHasPaymentsChange, mobileAction }: { token: string; purchaseId: number; onUnauthorized: () => void; onHasPaymentsChange?: (value: boolean) => void; mobileAction?: ReactNode }) {
  const [rows, setRows] = useState<PurchasePayment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setError('')
    void getPurchasePayments(token, purchaseId, controller.signal).then(value => {
      if (!controller.signal.aborted) { setRows(value); onHasPaymentsChange?.(value.length > 0) }
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) onUnauthorized()
      else setError(failure instanceof Error ? failure.message : 'No se pudieron consultar los pagos.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, purchaseId, revision, onUnauthorized, onHasPaymentsChange])
  return <section className={`purchase-payments-history${!loading && !error && rows.length === 0 ? ' is-empty' : ''}`} aria-label="Historial de pagos">
    <div className="payment-history-heading"><h3>Historial de pagos</h3>{!loading && !error && rows.length > 0 && <div className="payment-history-mobile-action">{mobileAction}</div>}</div>
    {loading ? <p role="status">Cargando pagos…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> : !rows.length ? <p>No hay pagos registrados.</p> :
      <div className="purchase-table-scroll purchase-payment-cards"><table><thead><tr><th>Fecha y hora</th><th>Usuario</th><th>Monto</th><th>Observación</th></tr></thead>
        <tbody>{rows.map(p => <tr key={p.idPago}><td data-label="Fecha y hora"><span className="payment-history-desktop-date">{new Date(p.fechaPago).toLocaleString('es-GT')}</span><span className="payment-history-mobile-date">{new Intl.DateTimeFormat('es-GT', { dateStyle: 'short' }).format(new Date(p.fechaPago))}</span></td><td data-label="Responsable">{p.usuario}</td><td data-label="Monto">{p.monto === null ? 'Sin información' : money.format(p.monto)}</td><td data-label="Observación">{p.observacion || '—'}</td></tr>)}</tbody>
      </table></div>}
  </section>
}
