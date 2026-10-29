const fs=require('node:fs'),path=require('node:path');
const root=path.resolve(__dirname,'../../Front-End.web/Front-End');
function write(file,s){fs.writeFileSync(path.join(root,file),s);}
write('src/features/sales/api/salePaymentsApi.ts', `import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type SalePayment = { idPago: number; idVenta: number; fechaPago: string; monto: number; metodoPago: string; observacion: string | null; usuario: string }
export async function loadSalePayments(saleId: number, token: string, signal?: AbortSignal): Promise<SalePayment[]> {
  if (!Number.isSafeInteger(saleId) || saleId <= 0) throw new ApiError('Venta inválida.', 400)
  const result = await requestJson('/Pagos/venta/' + saleId, { token, signal })
  if (!Array.isArray(result) || !result.every(v => v && typeof v === 'object' && Number.isSafeInteger(v.idPago) && v.idPago > 0 &&
    v.idVenta === saleId && typeof v.fechaPago === 'string' && !Number.isNaN(Date.parse(v.fechaPago)) &&
    typeof v.monto === 'number' && Number.isFinite(v.monto) && v.monto > 0 && typeof v.metodoPago === 'string' &&
    typeof v.usuario === 'string' && (v.observacion === null || typeof v.observacion === 'string')) ||
    new Set(result.map(v => v.idPago)).size !== result.length)
    throw new ApiError('El historial de pagos no tiene el formato esperado.')
  return result.map(v => ({ idPago: v.idPago, idVenta: v.idVenta, fechaPago: v.fechaPago, monto: v.monto,
    metodoPago: v.metodoPago, observacion: v.observacion, usuario: v.usuario }))
}
`);
write('src/features/sales/components/SalePaymentsHistory.tsx', `import { useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { loadSalePayments } from '../api/salePaymentsApi'
import type { SalePayment } from '../api/salePaymentsApi'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
export function SalePaymentsHistory({ saleId }: { saleId: number }) {
  const { session, logout } = useAuth()
  const token = session?.token
  const [items, setItems] = useState<SalePayment[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    setLoading(true); setError('')
    void loadSalePayments(saleId, token, controller.signal).then(result => { if (!controller.signal.aborted) setItems(result) })
      .catch(failure => {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setError(failure instanceof ApiError && failure.status === 404 ? 'La venta ya no está disponible.' : failure instanceof Error ? failure.message : 'No se pudieron cargar los pagos.')
      }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [saleId, token, logout, revision])
  return <section className="sale-payments-history" aria-label="Historial de pagos" aria-busy={loading}>
    <h3>Historial de pagos</h3>
    {loading ? <p role="status">Cargando pagos…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> :
      items.length === 0 ? <p role="status">Esta venta aún no tiene pagos registrados.</p> :
      <div className="sales-table-scroll" role="region" aria-label="Pagos de la venta" tabIndex={0}>
        <table><caption className="sr-only">Pago inicial y abonos de todas las sesiones de caja</caption>
          <thead><tr><th scope="col">Fecha y hora</th><th scope="col">Usuario</th><th scope="col">Método</th><th scope="col">Monto</th><th scope="col">Observación</th></tr></thead>
          <tbody>{items.map(payment => <tr key={payment.idPago}><td className="sale-list-nowrap">{date.format(new Date(payment.fechaPago))}</td>
            <td>{payment.usuario}</td><td>{payment.metodoPago}</td><td className="sale-list-money">{money.format(payment.monto)}</td><td>{payment.observacion || '—'}</td></tr>)}</tbody>
        </table>
      </div>}
  </section>
}
`);
const p=path.join(root,'src/features/sales/pages/SalePaymentForm.tsx');let s=fs.readFileSync(p,'utf8');
s="import { SalePaymentsHistory } from '../components/SalePaymentsHistory'\n"+s;
s=s.replace('  const paymentAmount = useRef', '  const paymentHeading = useRef<HTMLHeadingElement>(null)\n  const paymentAmount = useRef');
s=s.replace('panel?.showModal(); paymentAmount.current?.focus()', 'panel?.showModal(); if (sale.saldo_pendiente > 0) paymentAmount.current?.focus(); else paymentHeading.current?.focus()');
s=s.replace('  }, [paymentOpen])','  }, [paymentOpen, sale.saldo_pendiente])');
s=s.replace('{sale.saldo_pendiente > 0 && <button ref={paymentButton}', '<button ref={paymentButton}');
s=s.replace('disabled={saving || loading || !!detailError}\n            onClick={() => setPaymentOpen(true)} aria-haspopup="dialog">Abonar</button>}', 'disabled={saving}\n            onClick={() => setPaymentOpen(true)} aria-haspopup="dialog">{sale.saldo_pendiente > 0 ? \'Abonar\' : \'Pagos\'}</button>');
s=s.replace('<h2 id="sale-abono-title">Abonar · {sale.cliente}</h2>', '<h2 id="sale-abono-title" ref={paymentHeading} tabIndex={-1}>{sale.saldo_pendiente > 0 ? \'Abonar\' : \'Pagos\'} · {sale.cliente}</h2>');
s=s.replace('aria-label="Cerrar formulario de abono"', 'aria-label="Cerrar pagos"');
s=s.replace('    <form onSubmit={submit}>', '    <SalePaymentsHistory saleId={sale.id_ventas} />\n    {sale.saldo_pendiente > 0 && <form onSubmit={submit}>');
s=s.replace('    </form>\n    </section>', '    </form>}\n    </section>');
fs.writeFileSync(p,s);
fs.appendFileSync(path.join(root,'src/features/sales/pages/SaleListPage.css'), `
.sale-abono-dialog { width: min(850px, calc(100vw - 32px)); }
.sale-payments-history { margin-top: 16px; }
.sale-payments-history h3 { font-size: 1rem; margin: 0 0 12px; }
.sale-payments-history .sales-table-scroll { max-height: 38dvh; overflow: auto; }
.sale-payments-history th { position: sticky; top: 0; z-index: 1; }
`);
fs.appendFileSync(path.join(root,'tests/sales.test.ts'), `

import { loadSalePayments } from '../src/features/sales/api/salePaymentsApi.ts'
test('Historial de pagos consulta por venta con token sin limitar la sesión', async t => {
  t.mock.method(globalThis, 'fetch', async (url: string, options: RequestInit) => {
    assert.equal(url, '/api/Pagos/venta/12')
    assert.equal((options.headers as Record<string, string>).Authorization, 'Bearer test')
    return Response.json([{ idPago: 1, idVenta: 12, fechaPago: '2026-10-29T10:30:00', monto: 50, metodoPago: 'EFECTIVO', observacion: null, usuario: 'Ana', idSesionCaja: 99 }])
  })
  assert.equal((await loadSalePayments(12, 'test'))[0].monto, 50)
})
test('Historial de pagos admite venta sin pagos', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json([]))
  assert.deepEqual(await loadSalePayments(12, 'test'), [])
})
test('Historial rechaza pagos que pertenecen a otra venta', async t => {
  t.mock.method(globalThis, 'fetch', async () => Response.json([{ idPago: 1, idVenta: 13, fechaPago: '2026-10-29T10:30:00', monto: 50, metodoPago: 'EFECTIVO', observacion: null, usuario: 'Ana' }]))
  await assert.rejects(loadSalePayments(12, 'test'))
})
`);
