import { createPortal } from 'react-dom'
import type { ReactNode } from 'react'
import { useEffect, useState } from 'react'
import { WhatsAppIcon, WhatsAppReceiptDialog } from './WhatsAppReceipt'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { loadSalePayments } from '../api/salePaymentsApi'
import type { SalePayment } from '../api/salePaymentsApi'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
export function SalePaymentsHistory({ saleId, onHasPaymentsChange, mobileAction, mobileShareTarget }: { saleId: number; onHasPaymentsChange?: (value: boolean) => void; mobileAction?: ReactNode; mobileShareTarget?: HTMLElement | null }) {
  const { session, logout } = useAuth()
  const token = session?.token
  const [items, setItems] = useState<SalePayment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [sharing, setSharing] = useState(false)
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    setLoading(true); setError('')
    void loadSalePayments(saleId, token, controller.signal).then(result => { if (!controller.signal.aborted) { setItems(result); onHasPaymentsChange?.(result.length > 0) } })
      .catch(failure => {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setError(failure instanceof ApiError && failure.status === 404 ? 'La venta ya no está disponible.' : failure instanceof Error ? failure.message : 'No se pudieron cargar los pagos.')
      }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [saleId, token, logout, revision, onHasPaymentsChange])
  return <section className={`sale-payments-history${!loading && !error && items.length === 0 ? ' is-empty' : ''}`} aria-label="Historial de pagos" aria-busy={loading}>
    <div className="payment-history-heading"><h3>Historial de pagos</h3>{!loading && !error && items.length > 0 && <div className="payment-history-mobile-action">{mobileAction}</div>}</div>
    {mobileShareTarget ? createPortal(<button type="button" className="wa-send-button" disabled={loading || !!error || items.length === 0} onClick={() => setSharing(true)} aria-label="Compartir historial por WhatsApp" title="Compartir historial por WhatsApp"><WhatsAppIcon /></button>, mobileShareTarget) :
      <button type="button" className="wa-send-button sale-history-share-text" disabled={loading || !!error || items.length === 0} onClick={() => setSharing(true)}><WhatsAppIcon /> Compartir historial por WhatsApp</button>}
    {sharing && <WhatsAppReceiptDialog saleId={saleId} mode="pagos" onClose={() => setSharing(false)} />}
    {loading ? <p role="status">Cargando pagos…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> :
      items.length === 0 ? <p role="status">Esta venta aún no tiene pagos registrados.</p> :
      <div className="sales-table-scroll" role="region" aria-label="Pagos de la venta" tabIndex={0}>
        <table><caption className="sr-only">Pago inicial y abonos de todas las sesiones de caja</caption>
          <thead><tr><th scope="col">Fecha y hora</th><th scope="col">Usuario</th><th scope="col">Método</th><th scope="col">Monto</th><th scope="col">Observación</th></tr></thead>
          <tbody>{items.map(payment => <tr key={payment.idPago}><td data-label="Fecha y hora" className="sale-list-nowrap"><span className="payment-history-desktop-date">{date.format(new Date(payment.fechaPago))}</span><span className="payment-history-mobile-date">{new Intl.DateTimeFormat('es-GT', { dateStyle: 'short' }).format(new Date(payment.fechaPago))}</span></td>
            <td data-label="Responsable">{payment.usuario}</td><td data-label="Método">{payment.metodoPago}</td><td data-label="Monto" className="sale-list-money">{money.format(payment.monto)}</td><td data-label="Observación">{payment.observacion || '—'}</td></tr>)}</tbody>
        </table>
      </div>}
  </section>
}
