import { useEffect, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { getPurchasePayments } from '../api/purchasesApi'
import type { PurchasePayment } from '../api/purchasesApi'
const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PurchasePaymentsHistory({ token, purchaseId, onUnauthorized }: { token: string; purchaseId: number; onUnauthorized: () => void }) {
  const [rows, setRows] = useState<PurchasePayment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  useEffect(() => {
    const controller = new AbortController()
    setLoading(true); setError('')
    void getPurchasePayments(token, purchaseId, controller.signal).then(value => {
      if (!controller.signal.aborted) setRows(value)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) onUnauthorized()
      else setError(failure instanceof Error ? failure.message : 'No se pudieron consultar los pagos.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, purchaseId, revision, onUnauthorized])
  return <section className="purchase-payments-history" aria-label="Historial de pagos">
    <h3>Historial de pagos</h3>
    {loading ? <p role="status">Cargando pagos…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> : !rows.length ? <p>No hay pagos registrados.</p> :
      <div className="purchase-table-scroll"><table><thead><tr><th>Fecha y hora</th><th>Usuario</th><th>Monto</th><th>Observación</th></tr></thead>
        <tbody>{rows.map(p => <tr key={p.idPago}><td>{new Date(p.fechaPago).toLocaleString('es-GT')}</td><td>{p.usuario}</td><td>{p.monto === null ? 'Sin información' : money.format(p.monto)}</td><td>{p.observacion || '—'}</td></tr>)}</tbody>
      </table></div>}
  </section>
}
