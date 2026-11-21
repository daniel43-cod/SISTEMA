import type { PurchaseSummary } from '../api/purchasesApi'
import { MobilePurchaseCart } from '../components/MobilePurchaseCart'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { requestJson } from '../../../shared/api/httpClient'
import { parseCashSession } from '../../cash/api/cashApi'
import { loadProductNames } from '../../products/api/searchProducts'
import type { ProductSuggestion } from '../../products/api/searchProducts'
import { createPurchase, listSuppliers, getPurchaseSummary } from '../api/purchasesApi'
import { amountCents, buildPurchase } from '../utils/purchaseValidation'
import { PurchaseProductRow } from '../components/PurchaseProductRow'
import { PurchaseDetails } from '../components/PurchaseDetails'
import type { Purchase, PurchaseLine, Supplier } from '../types/purchase'
import './PurchasesPage.css'
import '../../../shared/ui/catalog.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PurchasesPage() {
  const { session, logout } = useAuth()
  const [desktopView, setDesktopView] = useState(() => window.matchMedia('(min-width: 801px)').matches)
  const [search, setSearch] = useState('')
  const [searchApplied, setSearchApplied] = useState('')
  const [statusFilter, setStatusFilter] = useState<'' | 'pendientes' | 'pagadas'>('')
  const [summary, setSummary] = useState<PurchaseSummary | null>(null)
  useEffect(() => {
    const media = window.matchMedia('(min-width: 801px)')
    const update = () => { setDesktopView(media.matches); setPage(1) }
    media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [])
  useEffect(() => {
    const timer = window.setTimeout(() => { setSearchApplied(search.trim()); setPage(1) }, 300)
    return () => window.clearTimeout(timer)
  }, [search])
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [products, setProducts] = useState<ProductSuggestion[]>([])
  const [purchases, setPurchases] = useState<Purchase[]>([])
  const [cashOpen, setCashOpen] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [page, setPage] = useState(1)
  const [supplier, setSupplier] = useState('')
  const [lines, setLines] = useState<PurchaseLine[]>([{ idProducto: 0, nombre: '', cantidad: '', total: '' }])
  const [note, setNote] = useState('')
  const [paid, setPaid] = useState('0')
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState('')
  const [uncertain, setUncertain] = useState(false)
  const [selected, setSelected] = useState<Purchase | null>(null)
  const [open, setOpen] = useState(false)
  const [purchaseStep, setPurchaseStep] = useState<'supplier' | 'products'>('supplier')
  const editor = useRef<HTMLElement>(null)
  const add = useRef<HTMLButtonElement>(null)
  useEffect(() => { if (open) { editor.current?.focus({ preventScroll: true }); editor.current?.scrollIntoView({ block: 'nearest' }) } }, [open])
  function closeEditor() {
    setOpen(false)
    requestAnimationFrame(() => add.current?.focus())
  }
  const busy = useRef(false)
  const unauthorized = useCallback(() => logout('La sesión ya no es válida.'), [logout])
  const token = session?.token
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError(''); setSummary(null)
      try {
        const [supplierRows, productRows, cash, history] = await Promise.all([
          listSuppliers(token!, controller.signal), loadProductNames(token!, controller.signal),
          requestJson('/Caja/actual', { token, signal: controller.signal }), getPurchaseSummary(token!, page, searchApplied, statusFilter, controller.signal),
        ])
        if (!controller.signal.aborted) {
          setSuppliers(supplierRows); setProducts(productRows); setCashOpen(parseCashSession(cash) !== null); setPurchases(history.items); setSummary(history)
        }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) unauthorized()
        else setError(failure instanceof Error ? failure.message : 'No se pudieron cargar las compras.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, page, revision, unauthorized, desktopView, searchApplied, statusFilter])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  const totalCents = lines.reduce((sum, line) => sum + (amountCents(line.total) ?? 0), 0)
  const paymentCents = amountCents(paid) ?? 0
  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current || uncertain || !cashOpen || loading || error || !token) return
    let payload
    try { payload = buildPurchase(supplier, note, paid, lines.filter(line => line.idProducto > 0 || line.nombre.trim())) }
    catch (failure) { setFormError(failure instanceof Error ? failure.message : 'Revisa la compra.'); return }
    busy.current = true; setSaving(true); setFormError('')
    try {
      await createPurchase(token, payload)
      setLines([{ idProducto: 0, nombre: '', cantidad: '', total: '' }]); setSupplier(''); setNote(''); setPaid('0')
      setPage(1); setRevision(v => v + 1)
      closeEditor()
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 401) unauthorized()
      else {
        const ambiguous = !(failure instanceof ApiError) || failure.status === 0 || failure.status >= 500 ||
          failure.message.includes('No se pudo confirmar')
        setUncertain(ambiguous)
        setFormError(ambiguous ? 'No se pudo confirmar el registro. Conservamos el formulario. Actualiza el historial y verifica si la compra existe antes de volver a enviarla.'
          : failure.message)
      }
    } finally { busy.current = false; setSaving(false) }
  }
  return <div className={`purchases-page ${open ? 'purchases-page-editing' : ''}`}>
    <div className="purchase-toolbar"><p role="status">{loading ? 'Consultando caja y compras…'
      : error ? 'No se pudo verificar la caja.' : cashOpen ? null : 'Abre la caja para registrar compras o abonos.'}</p>
    </div>
    {error && <p role="alert" className="purchase-error">{error}</p>}
    {open && <section ref={editor} tabIndex={-1} className={"purchase-panel purchase-editor-panel purchase-step-" + purchaseStep} aria-labelledby="new-purchase-title">
      <div className="purchase-panel-heading">
        <h2 id="new-purchase-title">Registrar compra</h2>
        <button type="button" aria-label="Cerrar formulario" title="Cerrar formulario" disabled={saving}
          onClick={closeEditor}>×</button>
      </div>
      <form onSubmit={submit}>
        <fieldset disabled={saving || loading || !cashOpen || !!error || uncertain} className="purchase-form">
          <legend className="purchase-sr-only">Datos de la compra</legend>
          <div className="purchase-fields purchase-supplier-card">
            <label>Proveedor<select id="purchase-supplier" value={supplier} onChange={e => setSupplier(e.target.value)} required>
              <option value="">Selecciona un proveedor</option>
              {suppliers.map(p => <option key={p.idProveedor} value={p.idProveedor}>{p.nombre}</option>)}
            </select></label>
            <label>Observación<input maxLength={100} value={note} onChange={e => setNote(e.target.value)} /></label>
          </div>
          <button type="button" className="purchase-step-continue purchase-primary" onClick={() => {
            const select = document.getElementById('purchase-supplier') as HTMLSelectElement | null
            if (!select?.reportValidity()) return
            setPurchaseStep('products')
            requestAnimationFrame(() => document.querySelector<HTMLInputElement>('.purchase-product-cell input')?.focus())
          }}>Continuar con productos →</button>
          <button type="button" className="purchase-step-back purchase-legacy-back" onClick={() => setPurchaseStep('supplier')}>← Ver proveedor</button>
          {!loading && !suppliers.length && <p>No hay proveedores registrados. Registra un proveedor para continuar.</p>}
          {purchaseStep === 'products' && <MobilePurchaseCart products={products} lines={lines} onChange={setLines}
            onClose={closeEditor} onSupplier={() => setPurchaseStep('supplier')} total={totalCents} paid={paid} onPaid={setPaid} saving={saving} />}
          <div className="purchase-table-scroll purchase-entry-cards"><table>
            <thead><tr><th>Producto</th><th>Unidades recibidas</th><th>Total de línea (Q)</th><th /></tr></thead>
            <tbody>{lines.map((line, index) => <PurchaseProductRow key={index} line={line} products={products}
              usedIds={lines.filter((_, position) => position !== index).map(row => row.idProducto)} autoFocus={index > 0}
              onChange={updated => setLines(current => current.map((row, position) => position === index ? updated : row))}
              onRemove={() => setLines(current => current.length === 1
                ? [{ idProducto: 0, nombre: '', cantidad: '', total: '' }] : current.filter((_, position) => position !== index))}
              onNext={() => {
                if (index < lines.length - 1) {
                  document.querySelectorAll<HTMLInputElement>('.purchase-product-cell input')[index + 1]?.focus()
                } else if (lines.length < 100) {
                  setLines(current => [...current, { idProducto: 0, nombre: '', cantidad: '', total: '' }])
                }
              }} />)}</tbody>
          </table></div>
          <div className="purchase-summary purchase-form-footer">
            <p>Total <strong>{money.format(totalCents / 100)}</strong></p>
            <label>Pago inicial (Q)<input inputMode="decimal" value={paid} onChange={e => setPaid(e.target.value)} required /></label>
            <p>Saldo pendiente <strong>{money.format(Math.max(0, totalCents - paymentCents) / 100)}</strong></p> <button className="purchase-primary" type="submit" disabled={!lines.some(line => line.idProducto > 0) || !suppliers.length}>{saving ? 'Registrando…' : 'Registrar compra'}</button>
          </div>
        </fieldset>
      </form>
      {formError && <p role="alert" className="purchase-error">{formError}</p>}
      {uncertain && <button type="button" onClick={() => { setUncertain(false); setFormError('') }}>
        Ya revisé el historial y la compra no se registró
      </button>}
    </section>}
    {(!open || uncertain) && <section className="purchase-panel purchase-history-panel" aria-label="Compras de la caja activa">
      {!desktopView && <>
        <section className="purchase-mobile-summary" aria-label="Resumen de compras de la caja activa">
          <div><span>Total de compras</span><strong>{summary ? money.format(summary.totalComprado) : '—'}</strong><small>{summary ? summary.totalRegistros + (summary.totalRegistros === 1 ? ' compra' : ' compras') : '—'}</small></div>
          <div><span>Por pagar</span><strong>{summary ? money.format(summary.totalPorPagar) : '—'}</strong><small>Caja activa</small></div>
        </section>
        <div className="purchase-mobile-status-tabs" role="group" aria-label="Filtrar compras de la caja activa">
          <button type="button" disabled={loading || saving} aria-pressed={statusFilter === ''} onClick={() => { setStatusFilter(''); setPage(1) }}>Todas ({summary ? summary.cantidadPendientes + summary.cantidadPagadas : '—'})</button>
          <button type="button" disabled={loading || saving} aria-pressed={statusFilter === 'pendientes'} onClick={() => { setStatusFilter('pendientes'); setPage(1) }}>Pendientes ({summary?.cantidadPendientes ?? '—'})</button>
          <button type="button" disabled={loading || saving} aria-pressed={statusFilter === 'pagadas'} onClick={() => { setStatusFilter('pagadas'); setPage(1) }}>Pagadas ({summary?.cantidadPagadas ?? '—'})</button>
        </div>
      </>}
      {desktopView && <>
        <div className="purchase-history-metrics">
          <article><span>Total comprado</span><strong>{summary ? money.format(summary.totalComprado) : '—'}</strong><small>{summary ? summary.totalRegistros + (summary.totalRegistros === 1 ? ' compra' : ' compras') : '—'}</small></article>
          <article className="purchase-history-debt"><span>Por pagar a distribuidores</span><strong>{summary ? money.format(summary.totalPorPagar) : '—'}</strong><small>{summary ? (statusFilter === 'pagadas' ? 0 : summary.cantidadPendientes) + ' con saldo' : '—'}</small></article>
          <article><span>Pagado</span><strong>{summary ? money.format(summary.totalPagado) : '—'}</strong><small>{summary && summary.totalComprado > 0 ? Math.round(summary.totalPagado / summary.totalComprado * 100) + ' % del total' : '0 % del total'}</small></article>
        </div>
        <div className="purchase-history-filters">
          <label>Buscar<input type="search" maxLength={100} placeholder="Distribuidor o número de compra" value={search} onChange={e => setSearch(e.target.value)} /></label>
          <button type="button" className="purchase-create-desktop" disabled={loading || saving || !!error || !cashOpen}
          onClick={() => { setSelected(null); setPurchaseStep('supplier'); setOpen(true) }}>+ Registrar compra</button>
          <RefreshButton label="Actualizar historial de compras" loading={saving || loading} onClick={() => setRevision(v => v + 1)} />
        </div>
        <div className="purchase-history-tabs" role="group" aria-label="Filtrar compras por estado">
          <button type="button" aria-pressed={statusFilter === ''} onClick={() => { setStatusFilter(''); setPage(1) }}>Todas ({summary ? summary.cantidadPendientes + summary.cantidadPagadas : '—'})</button>
          <button type="button" aria-pressed={statusFilter === 'pendientes'} onClick={() => { setStatusFilter('pendientes'); setPage(1) }}>Pendientes ({summary?.cantidadPendientes ?? '—'})</button>
          <button type="button" aria-pressed={statusFilter === 'pagadas'} onClick={() => { setStatusFilter('pagadas'); setPage(1) }}>Pagadas ({summary?.cantidadPagadas ?? '—'})</button>
        </div>
      </>}
      {loading ? <p role="status">Cargando compras…</p> : error ? <p>Actualiza para consultar el historial.</p>
        : !purchases.length ? <p>No hay compras en esta página.</p> : !desktopView ? <ul className="purchase-mobile-history" aria-label="Compras">
          {purchases.map(p => <li key={p.id_compra} className={p.saldo_pendiente > 0 ? 'is-pending' : undefined}>
            <button type="button" className="purchase-mobile-main" disabled={saving}
              aria-label={`Ver detalle de compra de ${p.nombre_proveedor}`} aria-haspopup="dialog"
              onClick={() => setSelected(p)}>
              <span className="purchase-mobile-name-row"><strong title={p.nombre_proveedor}>{p.nombre_proveedor}</strong>
                <span className={p.saldo_pendiente > 0 ? 'purchase-mobile-status is-pending' : 'purchase-mobile-status is-paid'}>{p.saldo_pendiente > 0 ? 'Pendiente' : 'Pagada'}</span>
              </span>
              <strong className="purchase-mobile-total">{money.format(p.total_compra)}</strong>
              {p.saldo_pendiente > 0 && <span className="purchase-mobile-balance">Saldo <strong>{money.format(p.saldo_pendiente)}</strong></span>}
            </button>
            <div className="purchase-mobile-options">
              <span className="purchase-mobile-seller" title={p.nombre_usuario}>{p.nombre_usuario}</span>
              <button type="button" className="purchase-mobile-more" disabled={saving} aria-haspopup="dialog"
                aria-label={`Ver detalle y opciones de compra de ${p.nombre_proveedor}`} title="Detalle y opciones"
                onClick={() => setSelected(p)}><span aria-hidden="true">⋮</span></button>
              <span className="purchase-mobile-time">{Number.isNaN(Date.parse(p.fecha_ingreso)) ? 'Hora no disponible' : new Intl.DateTimeFormat('es-GT', { hour: '2-digit', minute: '2-digit', hour12: true }).format(new Date(p.fecha_ingreso))}</span>
            </div>
          </li>)}
        </ul> : <div className="purchase-table-scroll purchase-history-cards"><table>
          <thead><tr>{desktopView && <th scope="col">Compra</th>}<th scope="col">Distribuidor</th><th scope="col">Usuario</th><th scope="col">Total</th><th scope="col">Saldo</th><th scope="col">Estado</th><th scope="col" className="purchase-history-date">Fecha y hora</th>{desktopView && <th scope="col">Acciones</th>}</tr></thead>
          <tbody>{purchases.map(p => <tr key={p.id_compra} className="purchase-history-row" tabIndex={saving ? -1 : 0} aria-label={`Ver compra de ${p.nombre_proveedor}`} onClick={() => { if (!saving) setSelected(p) }} onKeyDown={event => { if (!saving && (event.key === 'Enter' || event.key === ' ')) { event.preventDefault(); setSelected(p) } }}>
                        {desktopView && <td><strong>{p.id_compra}</strong><small className="purchase-history-row-date">{new Date(p.fecha_ingreso).toLocaleString('es-GT')}</small></td>}
            <td data-label="Proveedor">{p.nombre_proveedor}</td><td data-label="Responsable">{p.nombre_usuario}</td><td data-label="Total">{money.format(p.total_compra)}</td>
            <td data-label="Saldo pendiente" className={p.saldo_pendiente > 0 ? 'purchase-balance-pending' : undefined}>{money.format(p.saldo_pendiente)}</td><td data-label="Estado">{p.saldo_pendiente > 0 ? 'Pendiente' : 'Pagada'}</td>
            <td className="purchase-history-date" data-label="Fecha y hora"><time dateTime={p.fecha_ingreso}>{new Date(p.fecha_ingreso).toLocaleString('es-GT')}</time></td>
            {desktopView && <td><button type="button" disabled={saving} onKeyDown={e => e.stopPropagation()} onClick={e => { e.stopPropagation(); setSelected(p) }}>{p.saldo_pendiente > 0 ? "Registrar pago" : "Ver detalle"}</button></td>}
          </tr>)}</tbody>
        </table></div>}
      <nav className="purchase-pagination" aria-label="Páginas de compras">
        <button type="button" disabled={loading || saving || page === 1} onClick={() => { setSelected(null); setPage(p => p - 1) }}>Anterior</button>
        <span>Página {page}</span><button type="button" disabled={loading || saving || !!error || !summary?.hayMas}
          onClick={() => { setSelected(null); setPage(p => p + 1) }}>Siguiente</button>
      </nav>
    </section>}
    {!open && <button ref={add} type="button" className="catalog-add-button" aria-label="Registrar nueva compra"
      title="Registrar nueva compra" onClick={() => { setSelected(null); setPurchaseStep('supplier'); setOpen(true) }}>
      <span aria-hidden="true">+</span>
    </button>}
    {selected && token && <PurchaseDetails key={selected.id_compra} purchase={selected} token={token}
      canPay={cashOpen && !loading && !error} onClose={() => setSelected(null)}
      onPaid={() => { setSelected(null); setRevision(v => v + 1) }} onUnauthorized={unauthorized} />}
  </div>
}
