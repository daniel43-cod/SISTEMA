const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file, before, after) {
 const target = path.join(root, file), source = fs.readFileSync(target, 'utf8');
 if (!source.includes(before)) throw Error(file + ': texto no encontrado');
 fs.writeFileSync(target, source.replace(before, after));
}
const api = 'src/features/sales/api/salesApi.ts';
fs.appendFileSync(path.join(root, api), `
export type SaleDetail = { id_producto: number; nombre_producto: string; descripcion_resentacion: string; cantidad: number; precio: number; descuento: number }
export async function loadSaleDetails(saleId: number, token: string, signal?: AbortSignal): Promise<SaleDetail[]> {
  const result = await requestJson('/DetalleVenta_/listar/' + saleId, { token, signal })
  if (!Array.isArray(result) || !result.every(v => record(v) && id(v.id_producto) && typeof v.nombre_producto === 'string' &&
    typeof v.descripcion_resentacion === 'string' && id(v.cantidad) && amount(v.precio) && amount(v.descuento)))
    throw new ApiError('El detalle de venta no tiene el formato esperado.')
  return result as SaleDetail[]
}
export async function paySale(saleId: number, monto: number, observacion: string, token: string) {
  const result = await requestJson('/Venta/AbonarVenta', { method: 'POST', token, validationMessages: true,
    body: { id_venta: saleId, monto, observacion } })
  if (!record(result) || !id(result.id_pago) || result.id_venta !== saleId || result.monto !== monto)
    throw new ApiError('No se pudo confirmar la respuesta del abono.', 200)
}
`);
fs.writeFileSync(path.join(root, 'src/features/sales/pages/SalePaymentForm.tsx'), `import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { loadSaleDetails, paySale } from '../api/salesApi'
import type { SaleDetail, SaleHistory } from '../api/salesApi'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function SalePaymentForm({ sale, onClose, onPaid }: { sale: SaleHistory; onClose: () => void; onPaid: () => void }) {
  const { session, logout } = useAuth()
  const token = session?.token
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
    heading.current?.focus()
    heading.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
    return () => { alive.current = false }
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
    if (!/^\\d+(\\.\\d{1,2})?$/.test(input) || value <= 0 || Math.round(value * 100) > Math.round(sale.saldo_pendiente * 100)) {
      setError('Ingresa un monto mayor a cero, con hasta dos decimales y que no supere el saldo.'); return
    }
    busy.current = true; setSaving(true)
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
    } finally { busy.current = false; if (alive.current) setSaving(false) }
  }
  return <section className="sales-card sale-payment" aria-labelledby="sale-payment-title">
    <div className="sales-heading"><h2 id="sale-payment-title" ref={heading} tabIndex={-1}>Venta #{sale.id_ventas}</h2>
      <button type="button" disabled={saving} onClick={onClose}>Cerrar detalle</button></div>
    <dl className="sale-payment-summary"><div><dt>Cliente</dt><dd>{sale.cliente}</dd></div><div><dt>Total</dt><dd>{money.format(sale.total)}</dd></div>
      <div><dt>Saldo pendiente</dt><dd>{money.format(sale.saldo_pendiente)}</dd></div><div><dt>Estado</dt><dd>{sale.estado}</dd></div></dl>
    {loading ? <p role="status">Cargando detalle…</p> : detailError ? <div role="alert"><p>{detailError}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar detalle</button></div> :
      <div className="sales-table-scroll"><table><caption>Productos de la venta</caption><thead><tr><th scope="col">Producto</th><th scope="col">Presentación</th><th scope="col">Cantidad</th><th scope="col">Precio</th><th scope="col">Descuento</th><th scope="col">Subtotal</th></tr></thead>
        <tbody>{details.map((detail, index) => <tr key={index}><td>{detail.nombre_producto}</td><td>{detail.descripcion_resentacion}</td><td>{detail.cantidad}</td>
          <td>{money.format(detail.precio)}</td><td>{money.format(detail.descuento)}</td><td>{money.format((Math.round(detail.precio * 100) * detail.cantidad - Math.round(detail.descuento * 100)) / 100)}</td></tr>)}</tbody></table></div>}
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
}
`);
const page = 'src/features/sales/pages/SaleListPage.tsx';
edit(page, "import { useEffect, useState }", "import { SalePaymentForm } from './SalePaymentForm'\nimport { useEffect, useState }");
edit(page, "  const [page, setPage]", "  const [selected, setSelected] = useState<SaleHistory | null>(null)\n  const [success, setSuccess] = useState('')\n  const [page, setPage]");
edit(page, '    <div className="sales-card">', `    {success && <p role="status">{success}</p>}
    {selected && <SalePaymentForm key={selected.id_ventas} sale={selected}
      onClose={() => { setSelected(null); setRevision(v => v + 1) }}
      onPaid={() => { setSuccess('Abono registrado correctamente.'); setSelected(null); setRevision(v => v + 1) }} />}
    <div className="sales-card">`);
edit(page, '<tr key={sale.id_ventas}>', `<tr key={sale.id_ventas} className={selected?.id_ventas === sale.id_ventas ? 'sale-list-row is-selected' : 'sale-list-row'}
                  onClick={() => { setSelected(sale); setSuccess('') }}>`);
edit(page, '<td>#{sale.id_ventas}</td>', `<td><button type="button" aria-label={'Ver detalle y abonar venta ' + sale.id_ventas}
                    aria-expanded={selected?.id_ventas === sale.id_ventas} onClick={event => { event.stopPropagation(); setSelected(sale); setSuccess('') }}>#{sale.id_ventas}</button></td>`);
fs.appendFileSync(path.join(root, 'src/features/sales/pages/SaleListPage.css'), `
.sale-list-row { cursor: pointer; }
.sale-list-row:hover, .sale-list-row.is-selected { background: var(--surface-tint); }
.sale-payment { scroll-margin-top: 1rem; }
.sale-payment-summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 1rem; }
.sale-payment-summary dt { color: var(--text-muted); font-size: .85rem; }
.sale-payment-summary dd { margin: .4rem 0; font-weight: 600; }
.sale-payment form { margin-top: 1.5rem; }
.sale-payment-fields { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 1rem; }
.sale-payment legend { margin-bottom: 1rem; font-weight: 600; }
`);
