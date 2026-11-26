import { PendingCreation } from '../../../shared/api/PendingCreation'
import { catalogSaleLines } from '../utils/catalogSelection'
import { SaleCatalog } from '../components/SaleCatalog'
import type { SurtidoCatalogSelection } from '../../public-catalog/types/product'
import { useEffect, useId, useMemo, useRef, useState } from 'react'
import type { FormEvent, MouseEvent } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { createSale, loadSaleProducts } from '../api/salesApi'
import { loadSaleCustomers } from '../api/customersApi'
import type { SaleCustomer, SaleCustomerDetail } from '../api/customersApi'
import { SaleCustomerPicker } from '../components/SaleCustomerPicker'
import { SaleProductRow } from '../components/SaleProductRow'
import { MobileSaleCart } from '../components/MobileSaleCart'
import { SaleCheckoutDialog } from '../components/SaleCheckoutDialog'
import { SaleSurtidoEditor } from '../components/SaleSurtidoEditor'
import { saleOptions } from '../utils/saleSearch'
import type { SaleDraft, SaleOption } from '../utils/saleSearch'
import { cashCents, saleLineTotal, saleTotals } from '../utils/saleValidation'
import type { SaleLine, SaleProduct } from '../utils/saleValidation'
import './SalesPage.css'
import type { OrderDetail } from '../../orders/api/ordersApi'
import { confirmOrderSale } from '../../orders/api/ordersApi'
import { createOperationId } from '../utils/operationId'
import { WhatsAppReceiptDialog } from '../components/WhatsAppReceipt'

const money = (cents: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(cents / 100)
export function SalesPage({ order, onCancelOrder, onOrderConfirmed }: {
  order?: OrderDetail; onCancelOrder?: () => void; onOrderConfirmed?: () => void
} = {}) {
  const seeded = useRef(false)
  const paymentDialogRef = useRef<HTMLDialogElement>(null)
  const paymentConfirmation = useRef<((confirmed: boolean) => void) | null>(null)
  const paymentTitleId = useId()
  const paymentDescriptionId = useId()
  useEffect(() => () => { paymentConfirmation.current?.(false) }, [])
  function closePaymentConfirmation(confirmed: boolean) {
    paymentDialogRef.current?.close()
    paymentConfirmation.current?.(confirmed)
    paymentConfirmation.current = null
  }
  const surnameRef = useRef<HTMLInputElement>(null)
  const telRef = useRef<HTMLInputElement>(null)
  const emailRef = useRef<HTMLInputElement>(null)
  const addressRef = useRef<HTMLInputElement>(null)
  const saleFormId = useId()
  const customerFieldsId = useId()
  const [customerCollapsed, setCustomerCollapsed] = useState(false)
  const [mobileProductKey, setMobileProductKey] = useState<number | null>(null)
  const [mobileCartVisible, setMobileCartVisible] = useState(false)
  const [catalogOpen, setCatalogOpen] = useState(false)
  const entryRef = useRef<HTMLDivElement>(null)
  const productTableRef = useRef<HTMLDivElement>(null)
  useEffect(() => {
    const entry = entryRef.current
    const table = productTableRef.current
    if (!entry || !table) return
    const updateHeight = () => {
      table.style.setProperty('--sales-table-height', `${entry.clientHeight}px`)
    }
    updateHeight()
    const observer = new ResizeObserver(updateHeight)
    observer.observe(entry)
    return () => observer.disconnect()
  }, [])
  const productsInView = useRef(false)
  const { session, logout } = useAuth()
  const token = session?.token
  const [saleOperation] = useState(() => new PendingCreation<Record<string, unknown>>('create-sale-' + session?.user.id))
  const [products, setProducts] = useState<SaleProduct[]>([])
  const [customers, setCustomers] = useState<SaleCustomer[]>([])
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState('')
  const [revision, setRevision] = useState(0)
  const [rows, setRows] = useState<SaleDraft[]>([{ key: 0, text: '', selection: null }])
  const nextRowKey = useRef(1)
  const [surtidoEditor, setSurtidoEditor] = useState<{ option: SaleOption; key: number; initial?: SaleLine } | null>(null)
  const [pendingRows, setPendingRows] = useState<Set<number>>(() => new Set())
  const lines: SaleLine[] = rows.flatMap(row => row.selection ? [row.selection] : [])
  const options = useMemo(() => saleOptions(products), [products])
  const productPending = rows.some(row => pendingRows.has(row.key))
  const [customerId, setCustomerId] = useState('')
  const [name, setName] = useState(order?.nombreCliente ?? '')
  const [surname, setSurname] = useState('')
  const [address, setAddress] = useState(order?.direccion ?? '')
  const [phone, setPhone] = useState(order?.telefono ?? '')
  const [email, setEmail] = useState('')
  const [customerPending, setCustomerPending] = useState(false)
  const [customerSelectionFailed, setCustomerSelectionFailed] = useState(false)
  const [customerReset, setCustomerReset] = useState(0)
  const [received, setReceived] = useState('')
  const [checkoutOpen, setCheckoutOpen] = useState(false)
  const [note, setNote] = useState('')
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)
  const [receiptSaleId, setReceiptSaleId] = useState<number | null>(null)
  const [uncertain, setUncertain] = useState(!order && !!saleOperation.body)
  const busy = useRef(false)
  const operationId = useRef('')
  if (!operationId.current) operationId.current = createOperationId()
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    void (async () => {
      setLoading(true); setLoadError('')
      try {
        const [catalog, clients] = await Promise.all([loadSaleProducts(token, controller.signal), order ? Promise.resolve([] as SaleCustomer[]) : loadSaleCustomers(token, controller.signal)])
        if (!controller.signal.aborted) {
          setProducts(catalog); setCustomers(clients)
          if (order) {
            const makeRow = (line: OrderDetail['detalles'][number], index: number): SaleDraft => {
              const product = catalog.find(p => p.presentaciones.some(pr => pr.id_producto_presentacion === line.idProductoPresentacion))
              const presentation = product?.presentaciones.find(pr => pr.id_producto_presentacion === line.idProductoPresentacion)
              return { key: index, text: line.nombreProducto + ' · ' + line.nombrePresentacion,
                selection: product && presentation ? { product, presentation, quantity: String(line.cantidad), discount: '0.00',
                  ...(product.id_surtido ? { componentes: line.componentes?.map(c => ({ id_producto: c.idProducto, cantidad: c.cantidad })) } : {}) } : null }
            }
            if (!seeded.current) {
              setRows(order.detalles.map(makeRow)); nextRowKey.current = order.detalles.length; seeded.current = true
            } else {
              setRows(previous => previous.map(row => {
                if (!row.selection) return row
                const product = catalog.find(p => p.presentaciones.some(pr => pr.id_producto_presentacion === row.selection!.presentation.id_producto_presentacion))
                const presentation = product?.presentaciones.find(pr => pr.id_producto_presentacion === row.selection!.presentation.id_producto_presentacion)
                return { ...row, selection: product && presentation ? { ...row.selection, product, presentation } : null }
              }))
            }
          }
        }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setLoadError(failure instanceof Error ? failure.message : 'No se pudo cargar Ventas.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    })()
    return () => controller.abort()
  }, [token, revision, logout, order])
  let estimated: number | null = null
  try { estimated = lines.reduce((sum, line) => sum + saleLineTotal(line), 0) } catch { /* No mostrar totales válidos para un descuento inválido. */ }
  const receivedCents = received.trim() === '' ? 0 : cashCents(received)
  const blocked = pending || loading || !!loadError || uncertain
  const newCustomer = customerId === ''
  function selectCustomer(customer: SaleCustomerDetail | null) {
    setCustomerCollapsed(!!customer && window.matchMedia('(max-width: 800px)').matches)
    setCustomerSelectionFailed(false)
    setCustomerId(customer ? String(customer.id_cliente) : '')
    setName(customer?.nombre ?? ''); setSurname(customer?.apellido ?? '')
    setAddress(customer?.direccion ?? ''); setPhone(customer?.telefono ?? ''); setEmail(customer?.correo_electronico ?? '')
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
    if (option.product.id_surtido) { openSurtido(option, row.key); return false }
    const selection: SaleLine = { product: option.product, presentation: option.presentation, quantity: '1', discount: '0.00' }
    try {
      saleTotals([...rows.filter(other => other.key !== row.key).flatMap(other => other.selection ? [other.selection] : []), selection])
      setRows(previous => previous.some(other => other.key === row.key)
        ? previous.map(other => other.key === row.key ? { ...row, text: option.label, selection } : other)
        : [...previous, { ...row, text: option.label, selection }])
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
    if (blocked || customerPending || customerSelectionFailed) return
    setMobileProductKey(null)
    setMobileCartVisible(true)
    requestAnimationFrame(() => {
      entryRef.current?.scrollTo({ top: 0 })
      entryRef.current?.querySelector<HTMLInputElement>('[data-mobile-cart-search]')?.focus()
    })
  }
  function chooseCartProduct(option: SaleOption, quantity: string, discount: string) {
    if (blocked) return false
    const existing = rows.find(row => row.selection?.presentation.id_producto_presentacion === option.presentation.id_producto_presentacion)
    const empty = rows.find(row => !row.selection && !row.text.trim() && !row.barcode?.trim())
    const key = existing?.key ?? empty?.key ?? nextRowKey.current++
    const selected: SaleDraft = { key, text: option.label,
      selection: { product: option.product, presentation: option.presentation, quantity, discount } }
    const updated = rows.some(row => row.key === key)
      ? rows.map(row => row.key === key ? selected : row)
      : [...rows, selected]
    try {
      saleTotals(updated.flatMap(row => row.selection ? [row.selection] : []))
      setRows(updated); setError('')
      return true
    } catch (failure) { setError((failure as Error).message); return false }
  }
  function openSurtido(option: SaleOption, rowKey?: number) {
    if (blocked) return
    const existing = rows.find(r => r.selection?.presentation.id_producto_presentacion === option.presentation.id_producto_presentacion)
    const empty = rows.find(r => !r.selection && !r.text.trim() && !r.barcode?.trim())
    const key = existing?.key ?? rowKey ?? empty?.key ?? nextRowKey.current++
    setSurtidoEditor({ option, key, initial: existing?.selection ?? undefined })
  }
  function saveSurtido(line: SaleLine): string | null {
    if (!surtidoEditor || blocked) return 'La venta no está disponible.'
    const updated: SaleDraft = { key: surtidoEditor.key, text: surtidoEditor.option.label, selection: line }
    const next = rows.some(r => r.key === updated.key) ? rows.map(r => r.key === updated.key ? updated : r) : [...rows, updated]
    try { saleTotals(next.flatMap(r => r.selection ? [r.selection] : [])) }
    catch (failure) { return failure instanceof Error ? failure.message : 'Revisa las cantidades.' }
    setRows(next); setSurtidoEditor(null); setError(''); return null
  }
  function addFromCatalog(productId: number, selections: { presentationId: number; quantity: string }[]): string | null {
    if (blocked) return 'La venta no está disponible para agregar productos en este momento.'
    let added: SaleLine[]
    try {
      added = catalogSaleLines(productId, selections, options, lines)
    } catch (failure) { return failure instanceof Error ? failure.message : 'Revisa las cantidades y las existencias.' }
    const updated = [...rows]
    for (const selection of added) {
      const index = updated.findIndex(row => !row.selection && !row.text.trim() && !row.barcode?.trim() && !pendingRows.has(row.key))
      const key = index >= 0 ? updated[index].key : nextRowKey.current++
      const selected: SaleDraft = { key, text: selection.product.nombre + ' · ' + selection.presentation.presentacion, selection }
      if (index >= 0) updated[index] = selected
      else updated.push(selected)
    }
    setRows(updated)
    setError('')
    return null
  }
  function addSurtidosFromCatalog(idSurtido: number, selections: SurtidoCatalogSelection[]): string | null {
    if (blocked) return 'La venta no está disponible para agregar productos.'
    try {
      const added = selections.map(s => {
        if (lines.some(l => l.presentation.id_producto_presentacion === s.idProductoPresentacion))
          throw new Error('Esta presentación ya está agregada. Editala desde Nueva venta.')
        const option = options.find(o => o.product.id_surtido === idSurtido && o.presentation.id_producto_presentacion === s.idProductoPresentacion)
        if (!option) throw new Error('El surtido no está disponible. Actualiza el catálogo.')
        return { product: option.product, presentation: option.presentation, quantity: String(s.cantidad), discount: '0.00',
          componentes: s.componentes.map(c => ({ id_producto: c.idProducto, cantidad: c.cantidad })) } satisfies SaleLine
      })
      saleTotals([...lines, ...added])
      const next = [...rows]
      for (const selection of added) {
        const emptyIndex = next.findIndex(r => !r.selection && !r.text.trim() && !r.barcode?.trim())
        const row = { key: emptyIndex >= 0 ? next[emptyIndex].key : nextRowKey.current++, text: `${selection.product.nombre} · ${selection.presentation.presentacion}`, selection }
        if (emptyIndex >= 0) next[emptyIndex] = row; else next.push(row)
      }
      setRows(next); setError(''); return null
    } catch (failure) { return failure instanceof Error ? failure.message : 'Revisa las cantidades del surtido.' }
  }
  function changeCartDiscount(key: number, value: string) {
    if (blocked) return
    setRows(previous => previous.map(row => row.key === key && row.selection
      ? { ...row, selection: { ...row.selection, discount: value } } : row))
  }
  function changeCartQuantity(key: number, quantity: number) {
    if (blocked) return
    const current = rows.find(r => r.key === key)?.selection
    if (current?.product.id_surtido) {
      setSurtidoEditor({ option: { product: current.product, presentation: current.presentation, label: current.product.nombre }, key,
        initial: { ...current, quantity: String(quantity) } }); return
    }
    const updated = rows.map(row => row.key === key && row.selection
      ? { ...row, selection: { ...row.selection, quantity: String(quantity) } } : row)
    try {
      saleTotals(updated.flatMap(row => row.selection ? [row.selection] : []))
      setRows(updated); setError('')
    } catch (failure) { setError((failure as Error).message) }
  }
  function removeRow(key: number) {
    setRows(previous => previous.length === 1 ? [{ key: nextRowKey.current++, text: '', selection: null }] : previous.filter(row => row.key !== key))
    setPendingRows(previous => { const next = new Set(previous); next.delete(key); return next })
  }
  function setRowPending(key: number, pending: boolean) {
    setPendingRows(previous => { const next = new Set(previous); if (pending) next.add(key); else next.delete(key); return next })
  }
  function focusCustomerName() {
    setMobileCartVisible(false)
    setMobileProductKey(null)
    setCustomerCollapsed(false)
    productsInView.current = false
    setError('')
    requestAnimationFrame(() => {
      const input = entryRef.current?.querySelector<HTMLInputElement>('.sale-customer-picker input')
      input?.focus({ preventScroll: true })
      input?.scrollIntoView({ block: 'center', behavior: 'auto' })
    })
  }
  function checkCustomerBeforeRegister(event: MouseEvent<HTMLButtonElement>) {
    if (order) return
    if (!customerId && !name.trim()) {
      event.preventDefault()
      focusCustomerName()
      return
    }
    if (customerSelectionFailed) {
      event.preventDefault()
      setError('No se pudieron confirmar los datos del cliente. Selecciónalo nuevamente antes de registrar la venta.')
    }
  }
  function openCheckout() {
    if (blocked || customerPending || productPending) return
    if (!order && !customerId && !name.trim()) { focusCustomerName(); return }
    if (customerSelectionFailed) { setError('Selecciona nuevamente el cliente antes de cobrar.'); return }
    const form = document.getElementById(saleFormId) as HTMLFormElement | null
    if (!form?.reportValidity()) return
    try {
      if (rows.some(r => (r.text.trim() || r.barcode?.trim()) && !r.selection)) throw new Error('Selecciona un producto y presentación en cada fila que hayas escrito.')
      saleTotals(lines); setError(''); setCheckoutOpen(true)
    } catch (failure) { setError(failure instanceof Error ? failure.message : 'Revisa los productos de la venta.') }
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!token || busy.current || paymentConfirmation.current || blocked || customerPending || customerSelectionFailed || productPending) return
    if (!order && !customerId && !name.trim()) { focusCustomerName(); return }
    setError('')
    let total: number
    try {
      if (rows.some(row => (row.text.trim() || row.barcode?.trim()) && !row.selection)) throw new Error('Selecciona un producto y presentación en cada fila que hayas escrito.')
      total = saleTotals(lines)
      if (receivedCents === null) throw new Error('Ingresa un pago inicial válido, con hasta dos decimales.')
      if (!order && (newCustomer ? !name.trim() : !customers.some(c => String(c.id_cliente) === customerId))) throw new Error('Selecciona un cliente o ingresa su nombre.')
    } catch (failure) { setError((failure as Error).message); return }
    if (receivedCents === 0) {
      const confirmed = await new Promise<boolean>(resolve => {
        if (!paymentDialogRef.current) { resolve(false); return }
        paymentConfirmation.current = resolve
        paymentDialogRef.current.showModal()
      })
      if (!confirmed) return
    }
    busy.current = true; setPending(true)
    try {
      if (order) {
        const registered = await confirmOrderSale(order.idPedido, { idOperacion: operationId.current, totalEsperado: total / 100,
          pagoInicial: Math.min(receivedCents!, total) / 100,
          detalles: lines.map(l => ({ idProductoPresentacion: l.presentation.id_producto_presentacion, cantidad: Number(l.quantity), descuento: cashCents(l.discount?.trim() || '0.00')! / 100,
            ...(l.product.id_surtido ? { componentes: l.componentes!.map(c => ({ idProducto: c.id_producto, cantidad: c.cantidad })) } : {}) })) }, token)
        setCheckoutOpen(false); setReceiptSaleId(registered.id); return
      }
      const registered = await createSale(saleOperation.prepare({ id_operacion: operationId.current, id_cliente: newCustomer ? 0 : Number(customerId),
        ...(newCustomer ? { clienteNuevo: { nombre: name.trim(), apellido: surname.trim() || null, nit: null,
          direccion: address.trim() || null, dpi: null, telefono: phone.trim() || null, correo_electronico: email.trim() || null } } : {}),
        origen: 'MOSTRADOR', observacion: note.trim() || null,
        detalles: lines.map(l => ({ ...(l.product.id_surtido ? { id_surtido: l.product.id_surtido, componentes: l.componentes } : { id_producto: l.product.id_producto }),
          id_producto_presentacion: l.presentation.id_producto_presentacion, cantidad: Number(l.quantity), descuento: cashCents(l.discount?.trim() || '0.00')! / 100 })),
        pago: { monto: Math.min(receivedCents!, total) / 100, metodo_pago: 'EFECTIVO' } }), token)
      finishSale(registered.id)
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500 || failure.status === 200
      const keepOperation = order ? ambiguous : !!saleOperation.body && (ambiguous || (failure instanceof ApiError && (failure.status === 409 || failure.status === 403)))
      setUncertain(keepOperation)
      if (!order && !keepOperation) { saleOperation.complete(); operationId.current = createOperationId() }
      setError(ambiguous ? 'No se pudo confirmar la venta. Conservamos la solicitud: reintenta la misma operación sin duplicarla.'
        : failure instanceof ApiError && failure.status === 400 ? 'No se pudo registrar. Verifica caja abierta, permiso para vender, presentación activa y existencias; actualiza antes de continuar.' : (failure as Error).message)
    } finally { busy.current = false; setPending(false) }
  }
  function finishSale(id: number) {
    saleOperation.complete(); setUncertain(false)
      operationId.current = createOperationId()
      setRows([{ key: nextRowKey.current++, text: '', selection: null }]); setPendingRows(new Set()); setReceived(''); setNote(''); selectCustomer(null); setCustomerReset(v => v + 1)
      setRevision(v => v + 1)
      setCheckoutOpen(false)
      setReceiptSaleId(id)
  }
  async function retrySale() {
    if (!token || busy.current || !saleOperation.body) return
    busy.current = true; setPending(true); setError('')
    try {
      const registered = await createSale(saleOperation.prepare(saleOperation.body), token)
      finishSale(registered.id)
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      if (failure instanceof ApiError && [400, 404, 422].includes(failure.status)) {
        saleOperation.complete(); operationId.current = createOperationId(); setUncertain(false)
      }
      setError(failure instanceof Error ? failure.message : 'No se pudo confirmar la venta. Reintenta la misma operación.')
    } finally { busy.current = false; setPending(false) }
  }
  const customerFields = order ? (
    <>
    <fieldset id={customerFieldsId} className="sales-page sales-customer sales-customer-header" disabled>
      <label>Cliente<input value={order.nombreCliente} readOnly /></label>
      <label>Teléfono<input value={order.telefono} readOnly /></label>
      <label>Zona de entrega<input value={order.zonaEntrega} readOnly /></label>
      <label>Dirección<input value={order.direccion} readOnly /></label>
      {order.referencias && <label>Referencias<input value={order.referencias} readOnly /></label>}
      {order.observacion && <label>Observación del cliente<input value={order.observacion} readOnly /></label>}
    </fieldset>
    </>
  ) : (
<fieldset id={customerFieldsId} disabled={blocked} className={`sales-page sales-customer sales-customer-header sales-customer-form${customerCollapsed ? ' sales-customer-collapsed' : ''}`} onFocusCapture={() => { productsInView.current = false }}>
            <div className="sales-customer-group sales-customer-personal"><h3>Datos personales</h3>
            <SaleCustomerPicker key={customerReset} value={name} formId={saleFormId} customers={customers} token={token ?? ''} disabled={blocked}
              onSelect={selectCustomer} onPending={setCustomerPending} onNameChange={changeCustomerName}
              onNext={() => surnameRef.current?.focus()}
              onFailure={() => setCustomerSelectionFailed(true)}
              onUnauthorized={() => logout('La sesión ya no es válida.')} />
            <label>Apellido<span className="sales-customer-optional">opcional</span><input ref={surnameRef} form={saleFormId} value={surname} disabled={customerPending} maxLength={100} onChange={e => changeCustomerField(setSurname, e.target.value)} placeholder="Apellido (opcional)" 
            onKeyDown={event =>{
              if(event.key=='Enter' && !event.nativeEvent.isComposing){
                event.preventDefault()
                telRef.current?.focus()
              }
            }}
            /></label>
            </div>
            <div className="sales-customer-group sales-customer-contact"><h3>Contacto</h3>
            <label>Teléfono<span className="sales-customer-optional">opcional</span><input ref={telRef} form={saleFormId} value={phone} disabled={customerPending} maxLength={30} onChange={e => changeCustomerField(setPhone, e.target.value)} placeholder="Teléfono (opcional)"
              onKeyDown={event => {
                if (event.key === 'Enter' && !event.nativeEvent.isComposing) {
                  event.preventDefault()
                  emailRef.current?.focus()
                }
              }} /></label>
            <label>Correo electrónico<span className="sales-customer-optional">opcional</span><input ref={emailRef} form={saleFormId} value={email} disabled={customerPending} type="email" maxLength={254} onChange={e => changeCustomerField(setEmail, e.target.value)} placeholder="Correo (opcional)"
              onKeyDown={event => {
                if (event.key === 'Enter' && !event.nativeEvent.isComposing) {
                  event.preventDefault()
                  addressRef.current?.focus()
                }
              }} /></label>
            <label>Dirección<span className="sales-customer-optional">opcional</span><input ref={addressRef} form={saleFormId} value={address} disabled={customerPending} maxLength={200} onChange={e => changeCustomerField(setAddress, e.target.value)} placeholder="Dirección (opcional)"
              onKeyDown={event => {
                if (event.key === 'Enter' && !event.nativeEvent.isComposing) {
                  event.preventDefault()
                  productTableRef.current?.querySelector<HTMLInputElement>('.sale-barcode-cell input:not(:disabled)')?.focus()
                }
              }} /></label>
            </div>
          </fieldset>
  )
  return <div className={catalogOpen ? "sales-page sales-catalog-open" : "sales-page"}>
    {catalogOpen && <SaleCatalog options={options} lines={lines} disabled={blocked} onAdd={addFromCatalog} onAddSurtidos={addSurtidosFromCatalog}
      onClose={() => { setCatalogOpen(false); if (lines.length) { setMobileCartVisible(true); setMobileProductKey(null) } requestAnimationFrame(() => document.querySelector<HTMLButtonElement>(window.matchMedia("(max-width: 800px)").matches ? ".sales-cart-catalog-button" : ".sales-catalog-access button")?.focus()) }} />}
    {receiptSaleId !== null && <WhatsAppReceiptDialog saleId={receiptSaleId} mode="registro" onClose={() => {
      setReceiptSaleId(null); if (order) onOrderConfirmed?.()
    }} />}
    <dialog ref={paymentDialogRef} className="sales-payment-dialog" aria-labelledby={paymentTitleId} aria-describedby={paymentDescriptionId}
      onCancel={event => { event.preventDefault(); closePaymentConfirmation(false) }}>
      <h2 id={paymentTitleId}>Confirmar venta sin pago inicial</h2>
      <p id={paymentDescriptionId}>¿Estás seguro de que quieres registrar esta venta con un pago inicial de <strong>Q0.00</strong>?</p>
      <p>El total de <strong>{estimated === null ? '—' : money(estimated)}</strong> quedará como saldo pendiente.</p>
      <div className="sales-payment-dialog-actions">
        <button type="button" autoFocus onClick={() => closePaymentConfirmation(false)}>Cancelar</button>
        <button type="button" className="sales-primary" onClick={() => closePaymentConfirmation(true)}>Sí, registrar venta</button>
      </div>
    </dialog>
    <section className="sales-card" aria-labelledby="sale-title">
      <div className="sales-heading"><h2 id="sale-title">{order ? 'Preparar venta del pedido' : 'Nueva venta'}</h2>
        <button className="sales-mobile-customer-heading" type="button" aria-controls={customerFieldsId}
          disabled={pending} onClick={() => {
            setMobileCartVisible(false)
            setMobileProductKey(null)
            setCustomerCollapsed(false)
            productsInView.current = false
            entryRef.current?.scrollTo({ top: 0 })
          }}>Ver cliente</button>
        {order && <button type="button" disabled={pending} onClick={onCancelOrder}>Volver a pedidos</button>}<div className="sales-heading-actions"><div className="sales-catalog-access"><button type="button" disabled={blocked || productPending}
            aria-expanded={catalogOpen} onClick={() => setCatalogOpen(true)}>
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true"><path d="M3 3h7v7H3zM14 3h7v7h-7zM3 14h7v7H3zM14 14h7v7h-7z" /></svg>
            Catálogo
          </button></div><RefreshButton label="Actualizar catálogo y clientes" loading={loading || pending} onClick={() => setRevision(v => v + 1)} /></div></div>
      {order && <p>Revisa cantidades y precios actuales; puedes aplicar descuentos. El pedido original se conserva hasta confirmar la venta.</p>}
      {loading && <p role="status">Cargando catálogo y clientes…</p>}
      {loadError && <p className="sales-error" role="alert">{loadError}</p>}
      {error && <p className="sales-error" role="alert">{error}</p>}
      <form id={saleFormId} onSubmit={submit} onInvalidCapture={event => {
        if (!order && !customerId && !name.trim() && event.target instanceof HTMLInputElement &&
          event.target.closest('.sale-customer-picker')) {
          event.preventDefault()
          focusCustomerName()
        }
      }}>
        <fieldset disabled={blocked} className="sales-fields">
          <div className="sales-mobile-summary">
            <p aria-live="polite">Total <strong>{estimated === null ? '—' : money(estimated)}</strong></p>
            {mobileCartVisible && mobileProductKey === null && (
            <button type="button" className="sales-mobile-register-sale" onClick={openCheckout} aria-haspopup="dialog"
              disabled={blocked || !lines.length || estimated === null || customerPending || productPending}>
              Cobrar
            </button>
            )}
          </div>

          <div className={`sales-entry${mobileProductKey !== null ? ' sales-product-card-open' : ''}${mobileCartVisible && mobileProductKey === null ? ' sales-cart-open' : ''}${!mobileCartVisible && mobileProductKey === null && !order ? ' sales-customer-step' : ''}`} ref={entryRef}>
          {customerFields}
          <div className="sales-mobile-cart-region" hidden={!mobileCartVisible || mobileProductKey !== null}>
            <MobileSaleCart rows={rows} options={options} token={token ?? ''} disabled={blocked} onConfigureSurtido={openSurtido}
              onOpenCatalog={() => setCatalogOpen(true)} onChoose={chooseCartProduct}
              onQuantity={changeCartQuantity} onRemove={removeRow} onDiscount={changeCartDiscount}
              />

          </div>
          {mobileCartVisible === false && mobileProductKey === null && lines.length > 0 && <button type="button" className="sales-mobile-return-cart" onClick={() => { setMobileCartVisible(true); setCustomerCollapsed(false) }}>Ver carrito ({lines.length})</button>}
          {mobileProductKey !== null && <div className="sales-mobile-product-heading">
            <h3>Agregar producto</h3>
            <button type="button" onClick={() => setMobileProductKey(null)} aria-label="Volver a los datos del cliente">Volver</button>
          </div>}
          <div className="sales-table-scroll" ref={productTableRef}><table className="sales-line-table">
            <thead><tr><th scope="col">Código de barras</th><th scope="col">Producto y presentación</th><th scope="col">Cantidad</th><th scope="col">Precio</th><th scope="col">Descuento (Q)</th><th scope="col">Total</th><th scope="col"><span className="sr-only">Quitar</span></th></tr></thead>
            <tbody>{rows.map((row, index) => <SaleProductRow key={row.key} mobileActive={row.key === mobileProductKey} row={row} options={options}
              usedIds={rows.filter(other => other.key !== row.key).flatMap(other => other.selection ? [other.selection.presentation.id_producto_presentacion] : [])}
              token={token ?? ''} disabled={blocked} autoFocus={index > 0} onEditSurtido={row.selection?.product.id_surtido
                ? () => openSurtido({ product: row.selection!.product, presentation: row.selection!.presentation, label: row.text }, row.key) : undefined}
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
                  entry.scrollTo({ top: entry.scrollTop + table.getBoundingClientRect().top - entry.getBoundingClientRect().top,
                    behavior: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'instant' : 'smooth' })
                }
              }}
              onChoose={option => chooseRow(row, option)} onNext={() => nextRow(index)} onRemove={() => removeRow(row.key)}
              onPending={value => setRowPending(row.key, value)} />)}</tbody>
          </table></div>
          <button
  type="button"
  className="sales-add-product"
  onClick={addMobileRow}
  aria-label="Agregar producto"
  title="Agregar producto"
>
  <span aria-hidden="true">+</span>
</button>
          {mobileProductKey !== null && <button type="button" className="sales-mobile-product-done" disabled={blocked} onClick={() => {
            const row = rows.find(item => item.key === mobileProductKey)
            if (!row?.selection) { setError('Selecciona un producto y presentación.'); return }
            try {
              saleTotals(lines)
              setError('')
              setMobileProductKey(null)
              setMobileCartVisible(true)
            } catch (failure) { setError((failure as Error).message) }
          }}>Listo</button>}
          <label className="sales-observation">Observación<input value={note} maxLength={100} placeholder="Observación (opcional)" onChange={e => setNote(e.target.value)} /></label>
          </div>
          {!order && !mobileCartVisible && mobileProductKey === null && <div className="sales-customer-continue">
            <div><span>Total</span><strong>{estimated === null ? '—' : money(estimated)}</strong></div>
            <button type="button" disabled={blocked || customerPending || customerSelectionFailed} onClick={addMobileRow}>
              Continuar a productos <span aria-hidden="true">→</span>
            </button>
          </div>}
          <div className="sales-checkout"><p className="sales-checkout-total">Total:<strong>{estimated === null ? '—' : money(estimated)}</strong></p><label>Efectivo recibido (Q)<input inputMode="decimal" value={received} placeholder="0.00" onChange={e => setReceived(e.target.value)} /></label><p>Cambio:<strong>{estimated === null ? '—' : money(Math.max(0, (receivedCents ?? 0) - estimated))}</strong></p><button className="sales-primary" type="submit" onClick={checkCustomerBeforeRegister} disabled={!lines.length || customerPending || productPending}>{pending ? 'Registrando…' : order ? 'Confirmar y registrar venta' : 'Registrar venta'}</button></div>
        </fieldset>
      </form>
      {uncertain && (order ? <button type="button" onClick={() => { setUncertain(false); setError('') }}>Ya revisé el historial y confirmé que esta venta no se registró</button> : <button type="button" disabled={pending} onClick={() => void retrySale()}>Reintentar la misma venta</button>)}
    </section>

    {checkoutOpen && estimated !== null && <SaleCheckoutDialog total={estimated} received={received} onReceived={setReceived} pending={pending}
      blocked={blocked} error={error} onRegister={submit} onClose={() => { if (!pending) setCheckoutOpen(false) }} />}
    {surtidoEditor && <SaleSurtidoEditor option={surtidoEditor.option} initial={surtidoEditor.initial} disabled={blocked}
      existing={rows.filter(r => r.key !== surtidoEditor.key).flatMap(r => r.selection ? [r.selection] : [])} onSave={saveSurtido} onClose={() => setSurtidoEditor(null)} />}
  </div>
}
