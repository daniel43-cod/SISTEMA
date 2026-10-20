import { useEffect, useId, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { createSale, loadSaleProducts } from '../api/salesApi'
import { loadSaleCustomers } from '../api/customersApi'
import type { SaleCustomer, SaleCustomerDetail } from '../api/customersApi'
import { SaleCustomerPicker } from '../components/SaleCustomerPicker'
import { SaleProductRow } from '../components/SaleProductRow'
import { saleOptions } from '../utils/saleSearch'
import type { SaleDraft, SaleOption } from '../utils/saleSearch'
import { cashCents, saleLineTotal, saleTotals } from '../utils/saleValidation'
import type { SaleLine, SaleProduct } from '../utils/saleValidation'
import './SalesPage.css'
import { createOperationId } from '../utils/operationId'

const money = (cents: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(cents / 100)
export function SalesPage() {
  const saleFormId = useId()
  const customerFieldsId = useId()
  const [customerCollapsed, setCustomerCollapsed] = useState(false)
  const entryRef = useRef<HTMLDivElement>(null)
  const productTableRef = useRef<HTMLDivElement>(null)
  const productsInView = useRef(false)
  const { session, logout } = useAuth()
  const token = session?.token
  const [products, setProducts] = useState<SaleProduct[]>([])
  const [customers, setCustomers] = useState<SaleCustomer[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState('')
  const [revision, setRevision] = useState(0)
  const [rows, setRows] = useState<SaleDraft[]>([{ key: 0, text: '', selection: null }])
  const nextRowKey = useRef(1)
  const [pendingRows, setPendingRows] = useState<Set<number>>(() => new Set())
  const lines: SaleLine[] = rows.flatMap(row => row.selection ? [row.selection] : [])
  const options = useMemo(() => saleOptions(products), [products])
  const productPending = rows.some(row => pendingRows.has(row.key))
  const [customerId, setCustomerId] = useState('')
  const [name, setName] = useState('')
  const [surname, setSurname] = useState('')
  const [nit, setNit] = useState('')
  const [address, setAddress] = useState('')
  const [dpi, setDpi] = useState('')
  const [phone, setPhone] = useState('')
  const [email, setEmail] = useState('')
  const [customerPending, setCustomerPending] = useState(false)
  const [customerSelectionFailed, setCustomerSelectionFailed] = useState(false)
  const [customerReset, setCustomerReset] = useState(0)
  const [received, setReceived] = useState('')
  const [note, setNote] = useState('')
  const [error, setError] = useState('')
  const [notice, setNotice] = useState('')
  const [pending, setPending] = useState(false)
  const [uncertain, setUncertain] = useState(false)
  const busy = useRef(false)
  const operationId = useRef('')
  if (!operationId.current) operationId.current = createOperationId()
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    void (async () => {
      setLoading(true); setLoadError('')
      try {
        const [catalog, clients] = await Promise.all([loadSaleProducts(token, controller.signal), loadSaleCustomers(token, controller.signal)])
        if (!controller.signal.aborted) { setProducts(catalog); setCustomers(clients) }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setLoadError(failure instanceof Error ? failure.message : 'No se pudo cargar Ventas.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    })()
    return () => controller.abort()
  }, [token, revision, logout])
  let estimated: number | null = null
  try { estimated = lines.reduce((sum, line) => sum + saleLineTotal(line), 0) } catch { /* No mostrar totales válidos para un descuento inválido. */ }
  const receivedCents = cashCents(received)
  const blocked = pending || loading || !!loadError || uncertain
  const newCustomer = customerId === ''
  function selectCustomer(customer: SaleCustomerDetail | null) {
    setCustomerCollapsed(!!customer && window.matchMedia('(max-width: 800px)').matches)
    setCustomerSelectionFailed(false)
    setCustomerId(customer ? String(customer.id_cliente) : '')
    setName(customer?.nombre ?? ''); setSurname(customer?.apellido ?? ''); setNit(customer?.nit ?? '')
    setAddress(customer?.direccion ?? ''); setDpi(customer?.dpi ?? ''); setPhone(customer?.telefono ?? ''); setEmail(customer?.correo_electronico ?? '')
  }
  function changeCustomerName(value: string) {
    changeCustomerField(setName, value)
  }
  function changeCustomerField(setValue: (value: string) => void, value: string) {
    setCustomerId(''); setCustomerSelectionFailed(false); setValue(value)
  }
  function chooseRow(row: SaleDraft, option: SaleOption) {
    if (rows.some(other => other.key !== row.key && other.selection?.presentation.id_producto_presentacion === option.presentation.id_producto_presentacion)) {
      setError('Esa presentación ya está en otra fila. Cambia su cantidad en esa fila.'); return false
    }
    const selection: SaleLine = { product: option.product, presentation: option.presentation, quantity: '1', discount: '0.00' }
    try {
      saleTotals([...rows.filter(other => other.key !== row.key).flatMap(other => other.selection ? [other.selection] : []), selection])
      setRows(previous => previous.map(other => other.key === row.key ? { ...row, text: option.label, selection } : other))
      setError(''); return true
    } catch (failure) { setError((failure as Error).message); return false }
  }
  function addRow() {
    const key = nextRowKey.current++
    setRows(previous => [...previous, { key, text: '', selection: null }])
  }
  function nextRow(index: number) {
    if (index === rows.length - 1) addRow()
    else document.querySelector<HTMLInputElement>(`[data-sale-row="${rows[index + 1].key}"]`)?.focus()
  }
  function addMobileRow() {
    const emptyRow = rows.find(row => !row.selection && !row.text.trim() && !row.barcode?.trim())
    if (emptyRow) {
      entryRef.current?.querySelector<HTMLInputElement>(`[data-sale-row="${emptyRow.key}"]`)?.focus()
    } else {
      addRow()
    }
  }
  function removeRow(key: number) {
    setRows(previous => previous.length === 1 ? [{ key: nextRowKey.current++, text: '', selection: null }] : previous.filter(row => row.key !== key))
    setPendingRows(previous => { const next = new Set(previous); next.delete(key); return next })
  }
  function setRowPending(key: number, pending: boolean) {
    setPendingRows(previous => { const next = new Set(previous); if (pending) next.add(key); else next.delete(key); return next })
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || busy.current || blocked || customerPending || customerSelectionFailed || productPending) return
    setError(''); setNotice('')
    let total: number
    try {
      if (rows.some(row => (row.text.trim() || row.barcode?.trim()) && !row.selection)) throw new Error('Selecciona un producto y presentación en cada fila que hayas escrito.')
      total = saleTotals(lines)
      if (receivedCents === null || receivedCents < total) throw new Error('El efectivo recibido debe cubrir el total de la venta.')
      if (newCustomer ? !name.trim() : !customers.some(c => String(c.id_cliente) === customerId)) throw new Error('Selecciona un cliente o ingresa su nombre.')
    } catch (failure) { setError((failure as Error).message); return }
    busy.current = true; setPending(true)
    try {
      const confirmed = await createSale({ id_operacion: operationId.current, id_cliente: newCustomer ? 0 : Number(customerId),
        ...(newCustomer ? { clienteNuevo: { nombre: name.trim(), apellido: surname.trim() || null, nit: nit.trim() || null,
          direccion: address.trim() || null, dpi: dpi.trim() || null, telefono: phone.trim() || null, correo_electronico: email.trim() || null } } : {}),
        origen: 'MOSTRADOR', observacion: note.trim() || null,
        detalles: lines.map(l => ({ id_producto: l.product.id_producto, id_producto_presentacion: l.presentation.id_producto_presentacion, cantidad: Number(l.quantity), descuento: cashCents(l.discount ?? '0.00')! / 100 })),
        pago: { monto: total / 100, metodo_pago: 'EFECTIVO' } }, token)
      setNotice(`Venta #${confirmed.id} registrada. Total: ${money(Math.round(confirmed.total * 100))}. Saldo: ${money(Math.round(confirmed.saldo * 100))}. Cambio: ${money(receivedCents! - total)}.`)
      operationId.current = createOperationId()
      setRows([{ key: nextRowKey.current++, text: '', selection: null }]); setPendingRows(new Set()); setReceived(''); setNote(''); selectCustomer(null); setCustomerReset(v => v + 1)
      setRevision(v => v + 1)
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500 || failure.status === 200
      setUncertain(ambiguous)
      setError(ambiguous ? 'No se pudo confirmar la venta. Conservamos los datos. Actualiza y revisa el historial antes de intentar nuevamente.'
        : failure instanceof ApiError && failure.status === 400 ? 'No se pudo registrar. Verifica caja abierta, permiso para vender, presentación activa y existencias; actualiza antes de continuar.' : (failure as Error).message)
    } finally { busy.current = false; setPending(false) }
  }
  const customerFields = (
<fieldset id={customerFieldsId} disabled={blocked} className={`sales-page sales-customer sales-customer-header${customerCollapsed ? ' sales-customer-collapsed' : ''}`} onFocusCapture={() => { productsInView.current = false }}>
            <SaleCustomerPicker key={customerReset} value={name} formId={saleFormId} customers={customers} token={token ?? ''} disabled={blocked}
              onSelect={selectCustomer} onPending={setCustomerPending} onNameChange={changeCustomerName}
              onFailure={() => setCustomerSelectionFailed(true)}
              onUnauthorized={() => logout('La sesión ya no es válida.')} />
            <label>Apellido<input form={saleFormId} value={surname} disabled={customerPending} maxLength={100} onChange={e => changeCustomerField(setSurname, e.target.value)} placeholder="Apellido (opcional)" /></label>
            <label>NIT<input form={saleFormId} value={nit} disabled={customerPending} maxLength={20} onChange={e => changeCustomerField(setNit, e.target.value)} placeholder="NIT (opcional)" /></label>
            <label>DPI<input form={saleFormId} value={dpi} disabled={customerPending} maxLength={20} onChange={e => changeCustomerField(setDpi, e.target.value)} placeholder="DPI (opcional)" /></label>
            <label>Teléfono<input form={saleFormId} value={phone} disabled={customerPending} maxLength={30} onChange={e => changeCustomerField(setPhone, e.target.value)} placeholder="Teléfono (opcional)" /></label>
            <label>Correo electrónico<input form={saleFormId} value={email} disabled={customerPending} type="email" maxLength={254} onChange={e => changeCustomerField(setEmail, e.target.value)} placeholder="Correo (opcional)" /></label>
            <label>Dirección<input form={saleFormId} value={address} disabled={customerPending} maxLength={200} onChange={e => changeCustomerField(setAddress, e.target.value)} placeholder="Dirección (opcional)" /></label>
          </fieldset>
  )
  return <div className="sales-page">
    <section className="sales-card" aria-labelledby="sale-title">
      <div className="sales-heading"><h2 id="sale-title">Nueva venta</h2><RefreshButton label="Actualizar catálogo y clientes" loading={loading || pending} onClick={() => setRevision(v => v + 1)} /></div>
      {notice && <p className="sales-success" role="status">{notice}</p>}
      {loading && <p role="status">Cargando catálogo y clientes…</p>}
      {loadError && <p className="sales-error" role="alert">{loadError}</p>}
      {error && <p className="sales-error" role="alert">{error}</p>}
      <form id={saleFormId} onSubmit={submit}>
        <fieldset disabled={blocked} className="sales-fields">
          <div className="sales-mobile-summary">
          <button className="sales-view-customer" type="button" aria-expanded={!customerCollapsed} aria-controls={customerFieldsId} onClick={() => {
            setCustomerCollapsed(value => !value)
            productsInView.current = false
            entryRef.current?.scrollTo({ top: 0 })
          }}>{customerCollapsed ? 'Ver cliente' : 'Ocultar'}</button>
          <p>Total <strong>{estimated === null ? '—' : money(estimated)}</strong></p>
          </div>
          <div className="sales-entry" ref={entryRef}>
          {customerFields}
          <div className="sales-table-scroll" ref={productTableRef}><table className="sales-line-table">
            <thead><tr><th scope="col">Código de barras</th><th scope="col">Producto y presentación</th><th scope="col">Cantidad</th><th scope="col">Precio</th><th scope="col">Descuento (Q)</th><th scope="col">Total</th><th scope="col"><span className="sr-only">Quitar</span></th></tr></thead>
            <tbody>{rows.map((row, index) => <SaleProductRow key={row.key} row={row} options={options}
              usedIds={rows.filter(other => other.key !== row.key).flatMap(other => other.selection ? [other.selection.presentation.id_producto_presentacion] : [])}
              token={token ?? ''} disabled={blocked} autoFocus={index > 0}
              onChange={updated => {
                setRows(previous => previous.map(other => other.key === row.key ? updated : other))
                const entry = entryRef.current
                const table = productTableRef.current
                if (((updated.text.trim() && updated.text !== row.text) || (updated.barcode?.trim() && updated.barcode !== row.barcode)) && window.matchMedia('(max-width: 800px)').matches) {
                  setCustomerCollapsed(true)
                  return
                }
                if (!productsInView.current && name.trim() && updated.text.trim() && updated.text !== row.text && entry && table) {
                  productsInView.current = true
                  table.style.minHeight = `${entry.clientHeight}px`
                  entry.scrollTo({ top: entry.scrollTop + table.getBoundingClientRect().top - entry.getBoundingClientRect().top,
                    behavior: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' })
                }
              }}
              onChoose={option => chooseRow(row, option)} onNext={() => nextRow(index)} onRemove={() => removeRow(row.key)}
              onPending={value => setRowPending(row.key, value)} />)}</tbody>
          </table></div>
          <button type="button" className="sales-add-product" onClick={addMobileRow}>+ Agregar producto</button>
          <label className="sales-observation">Observación<input value={note} maxLength={100} placeholder="Observación (opcional)" onChange={e => setNote(e.target.value)} /></label>
          </div>
          <div className="sales-checkout"><p className="sales-checkout-total">Total<strong>{estimated === null ? '—' : money(estimated)}</strong></p><label>Efectivo recibido (Q)<input inputMode="decimal" value={received} required placeholder="0.00" onChange={e => setReceived(e.target.value)} /></label><p>Cambio<strong>{estimated === null ? '—' : money(Math.max(0, (receivedCents ?? 0) - estimated))}</strong></p><button className="sales-primary" type="submit" disabled={!lines.length || customerPending || customerSelectionFailed || productPending || (!customerId && !name.trim())}>{pending ? 'Registrando…' : 'Registrar venta'}</button></div>
        </fieldset>
      </form>
      {uncertain && <button type="button" onClick={() => { setUncertain(false); setError('') }}>Ya revisé el historial y confirmé que esta venta no se registró</button>}
    </section>

  </div>
}
