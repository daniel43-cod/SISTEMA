const fs=require('fs');const root='C:/Users/coy81/Desktop/SISTEMA/SISTEMA/Front-End.web/Front-End/src/features/purchases/';
fs.appendFileSync(root+'api/purchasesApi.ts', `
export type PurchasePayment = { idPago: number; idCompra: number; fechaPago: string; monto: number | null; usuario: string; observacion: string | null }
export async function getPurchasePayments(token: string, purchaseId: number, signal?: AbortSignal): Promise<PurchasePayment[]> {
  const result = await requestJson('/Compra/' + purchaseId + '/pagos', { token, signal })
  if (!Array.isArray(result) || !result.every(p => object(p) && id(p.idPago) && p.idCompra === purchaseId &&
    typeof p.fechaPago === 'string' && !Number.isNaN(Date.parse(p.fechaPago)) &&
    (p.monto === null || (typeof p.monto === 'number' && Number.isFinite(p.monto) && p.monto > 0)) &&
    typeof p.usuario === 'string' && (p.observacion === null || typeof p.observacion === 'string')))
    throw new ApiError('El historial de pagos no tiene el formato esperado.')
  return result as PurchasePayment[]
}
`);
fs.writeFileSync(root+'components/PurchasePaymentsHistory.tsx', `import { useEffect, useState } from 'react'
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
`);
const p=root+'components/PurchaseDetails.tsx';let s=fs.readFileSync(p,'utf8');s="import { PurchasePaymentsHistory } from './PurchasePaymentsHistory'\n"+s;
s=s.replace('  const dialogRef = useRef<HTMLDialogElement>(null)', `  const dialogRef = useRef<HTMLDialogElement>(null)
  const paymentsDialog = useRef<HTMLDialogElement>(null)
  const paymentsTrigger = useRef<HTMLButtonElement>(null)
  const [paymentsOpen, setPaymentsOpen] = useState(false)
  useEffect(() => {
    if (!paymentsOpen) return
    const dialog = paymentsDialog.current
    dialog?.showModal()
    return () => { dialog?.close(); paymentsTrigger.current?.focus() }
  }, [paymentsOpen])`);
s=s.replace('return createPortal(<dialog ref={dialogRef}', 'return createPortal(<><dialog ref={dialogRef}');
s=s.replace('    {loading && <p role="status">', `    <div className="purchase-payment-actions">
      <span className={'purchase-payment-status ' + (purchase.saldo_pendiente > 0 ? 'is-pending' : 'is-paid')}>{purchase.saldo_pendiente > 0 ? 'Pendiente' : 'Pagada'}</span>
      <button type="button" ref={paymentsTrigger} onClick={() => setPaymentsOpen(true)}>{purchase.saldo_pendiente > 0 ? 'Abonar' : 'Pagos'}</button>
    </div>
    {loading && <p role="status">`);
const a=s.indexOf('    {(purchase.saldo_pendiente > 0 || uncertain) && <form');const b=s.indexOf('  </dialog>, document.body)',a);if(a<0||b<0)throw Error('Missing form');
const form=s.slice(a,b);
s=s.slice(0,a)+`  </dialog>
  {paymentsOpen && <dialog ref={paymentsDialog} className="purchases-page purchase-panel purchase-detail-dialog purchase-payments-dialog" aria-labelledby="purchase-payments-title"
    onCancel={event => { event.preventDefault(); if (!busy.current) setPaymentsOpen(false) }}>
    <div className="purchase-panel-heading"><h2 id="purchase-payments-title">{purchase.saldo_pendiente > 0 ? 'Abonar' : 'Pagos'} · {purchase.nombre_proveedor}</h2>
      <button type="button" className="purchase-detail-close" aria-label="Cerrar pagos" disabled={saving} onClick={() => setPaymentsOpen(false)}>×</button></div>
    <p>Saldo pendiente: <strong>{money.format(purchase.saldo_pendiente)}</strong></p>
    <PurchasePaymentsHistory token={token} purchaseId={purchase.id_compra} onUnauthorized={onUnauthorized} />
    {error && <p role="alert" className="purchase-error">{error}</p>}
    {!canPay && purchase.saldo_pendiente > 0 && !uncertain && <p>Abre la caja para poder abonar la compra.</p>}
`+form+`  </dialog>}</>, document.body)
}
`;
fs.writeFileSync(p,s);
const css=root+'pages/PurchasesPage.css';fs.appendFileSync(css, `
.purchase-payment-actions { display: flex; align-items: center; gap: 12px; margin: 8px 0 16px; }
.purchase-payment-status { padding: 6px 10px; border-radius: 12px; font-size: .875rem; }
.purchase-payment-status.is-paid { background: #dcfce7; color: #166534; }
.purchase-payment-status.is-pending { background: #ffedd5; color: #9a3412; }
.purchase-payments-dialog { width: min(800px, calc(100vw - 32px)); }
.purchase-payments-history h3 { margin: 12px 0; font-size: 1rem; }
.purchase-payments-history .purchase-table-scroll { max-height: 32dvh; overflow: auto; }
`);
