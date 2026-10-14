import { useCallback, useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { requestJson } from '../../../shared/api/httpClient'
import { parseCashSession } from '../../cash/api/cashApi'
import { loadProductNames, filterProductNames } from '../../products/api/searchProducts'
import type { ProductSuggestion } from '../../products/api/searchProducts'
import { createPurchase, listPurchases, listSuppliers } from '../api/purchasesApi'
import { amountCents, buildPurchase } from '../utils/purchaseValidation'
import { PurchaseDetails } from '../components/PurchaseDetails'
import type { CreatedPurchase, Purchase, PurchaseLine, Supplier } from '../types/purchase'
import './PurchasesPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PurchasesPage() {
  const { session, logout } = useAuth()
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [products, setProducts] = useState<ProductSuggestion[]>([])
  const [purchases, setPurchases] = useState<Purchase[]>([])
  const [cashOpen, setCashOpen] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [page, setPage] = useState(1)
  const [supplier, setSupplier] = useState('')
  const [query, setQuery] = useState('')
  const [lines, setLines] = useState<PurchaseLine[]>([])
  const [note, setNote] = useState('')
  const [paid, setPaid] = useState('0')
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState('')
  const [uncertain, setUncertain] = useState(false)
  const [result, setResult] = useState<CreatedPurchase | null>(null)
  const [selected, setSelected] = useState<Purchase | null>(null)
  const busy = useRef(false)
  const unauthorized = useCallback(() => logout('La sesión ya no es válida.'), [logout])
  const token = session?.token
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    async function load() {
      setLoading(true); setError('')
      try {
        const [supplierRows, productRows, cash, history] = await Promise.all([
          listSuppliers(token!, controller.signal), loadProductNames(token!, controller.signal),
          requestJson('/Caja/actual', { token, signal: controller.signal }), listPurchases(token!, page, controller.signal),
        ])
        if (!controller.signal.aborted) {
          setSuppliers(supplierRows); setProducts(productRows); setCashOpen(parseCashSession(cash) !== null); setPurchases(history)
        }
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) unauthorized()
        else setError(failure instanceof Error ? failure.message : 'No se pudieron cargar las compras.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, page, revision, unauthorized])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  const suggestions = filterProductNames(products, query).filter(p => !lines.some(line => line.idProducto === p.idProducto))
  const totalCents = lines.reduce((sum, line) => sum + (amountCents(line.total) ?? 0), 0)
  const paymentCents = amountCents(paid) ?? 0
  function editLine(id: number, field: 'cantidad' | 'total', value: string) {
    setLines(current => current.map(line => line.idProducto === id ? { ...line, [field]: value } : line))
  }
  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (busy.current || uncertain || !cashOpen || loading || error || !token) return
    let payload
    try { payload = buildPurchase(supplier, note, paid, lines) }
    catch (failure) { setFormError(failure instanceof Error ? failure.message : 'Revisa la compra.'); return }
    busy.current = true; setSaving(true); setFormError(''); setResult(null)
    try {
      const created = await createPurchase(token, payload)
      setResult(created); setLines([]); setSupplier(''); setNote(''); setPaid('0'); setQuery('')
      setPage(1); setRevision(v => v + 1)
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
  return <div className="purchases-page">
    <div className="purchase-toolbar"><p role="status">{loading ? 'Consultando caja y compras…'
      : error ? 'No se pudo verificar la caja.' : cashOpen ? 'Caja abierta' : 'Abre la caja para registrar compras o abonos.'}</p>
      <button type="button" disabled={saving || loading} onClick={() => setRevision(v => v + 1)}>Actualizar</button></div>
    {error && <p role="alert" className="purchase-error">{error}</p>}
    {result && <section className="purchase-success" role="status">
      <h2>Compra #{result.id_compra} registrada</h2>
      <p>Total: {money.format(result.total_compra)} · Saldo: {money.format(result.saldo_pendiente)}</p>
      <ul>{result.productos.map(p => <li key={p.idProducto}>{products.find(row => row.idProducto === p.idProducto)?.nombre ?? `Producto #${p.idProducto}`}
        : {p.stock} unidades · Costo promedio: {p.precioPromedioCompra.toFixed(6)}</li>)}</ul>
    </section>}
    <section className="purchase-panel" aria-labelledby="new-purchase-title">
      <h2 id="new-purchase-title">Registrar compra</h2>
      <p>Ingresa las unidades recibidas y el total pagado por cada producto. Ejemplo: 10 bandejas de 6 botellas son 60 unidades.</p>
      <form onSubmit={submit}>
        <fieldset disabled={saving || loading || !cashOpen || !!error || uncertain} className="purchase-form">
          <legend className="purchase-sr-only">Datos de la compra</legend>
          <div className="purchase-fields">
            <label>Proveedor<select value={supplier} onChange={e => setSupplier(e.target.value)} required>
              <option value="">Selecciona un proveedor</option>
              {suppliers.map(p => <option key={p.idProveedor} value={p.idProveedor}>{p.nombre}</option>)}
            </select></label>
            <label>Observación<input maxLength={100} value={note} onChange={e => setNote(e.target.value)} /></label>
          </div>
          {!loading && !suppliers.length && <p>No hay proveedores registrados. Registra un proveedor para continuar.</p>}
          <label>Buscar producto<input type="search" value={query} maxLength={200}
            placeholder="Escribe al menos 3 letras" onChange={e => setQuery(e.target.value)} /></label>
          {suggestions.length > 0 && <ul className="purchase-suggestions">{suggestions.map(p => <li key={p.idProducto}>
            <button type="button" disabled={lines.length >= 100} onClick={() => {
              setLines(current => current.some(row => row.idProducto === p.idProducto) ? current
                : [...current, { idProducto: p.idProducto, nombre: p.nombre, cantidad: '', total: '' }]); setQuery('')
            }}>{p.nombre} <span>+ Agregar</span></button>
          </li>)}</ul>}
          {query.trim().length >= 3 && !suggestions.length && <p role="status">No hay productos nuevos que coincidan con la búsqueda.</p>}
          {lines.length > 0 ? <div className="purchase-table-scroll"><table>
            <thead><tr><th>Producto</th><th>Unidades recibidas</th><th>Total de línea (Q)</th><th /></tr></thead>
            <tbody>{lines.map(line => <tr key={line.idProducto}>
              <td>{line.nombre}</td>
              <td><input aria-label={`Unidades de ${line.nombre}`} inputMode="numeric" value={line.cantidad}
                onChange={e => editLine(line.idProducto, 'cantidad', e.target.value)} required /></td>
              <td><input aria-label={`Total de ${line.nombre}`} inputMode="decimal" value={line.total}
                onChange={e => editLine(line.idProducto, 'total', e.target.value)} required /></td>
              <td><button type="button" aria-label={`Quitar ${line.nombre}`}
                onClick={() => setLines(current => current.filter(row => row.idProducto !== line.idProducto))}>Quitar</button></td>
            </tr>)}</tbody>
          </table></div> : <p>Agrega productos para registrar la compra.</p>}
          <div className="purchase-summary">
            <p>Total <strong>{money.format(totalCents / 100)}</strong></p>
            <label>Pago inicial (Q)<input inputMode="decimal" value={paid} onChange={e => setPaid(e.target.value)} required /></label>
            <p>Saldo pendiente <strong>{money.format(Math.max(0, totalCents - paymentCents) / 100)}</strong></p>
          </div>
          <button className="purchase-primary" type="submit" disabled={!lines.length || !suppliers.length}>{saving ? 'Registrando…' : 'Registrar compra'}</button>
        </fieldset>
      </form>
      {formError && <p role="alert" className="purchase-error">{formError}</p>}
      {uncertain && <button type="button" onClick={() => { setUncertain(false); setFormError('') }}>
        Ya revisé el historial y la compra no se registró
      </button>}
    </section>
    <section className="purchase-panel" aria-labelledby="purchase-history-title">
      <h2 id="purchase-history-title">Historial de compras</h2>
      {loading ? <p role="status">Cargando compras…</p> : error ? <p>Actualiza para consultar el historial.</p>
        : !purchases.length ? <p>No hay compras en esta página.</p> : <div className="purchase-table-scroll"><table>
          <thead><tr><th>Compra / Fecha</th><th>Proveedor</th><th>Total</th><th>Saldo</th><th>Estado</th><th /></tr></thead>
          <tbody>{purchases.map(p => <tr key={p.id_compra}>
            <td>#{p.id_compra}<small>{new Date(p.fecha_ingreso).toLocaleString('es-GT')}</small></td>
            <td>{p.nombre_proveedor}</td><td>{money.format(p.total_compra)}</td>
            <td>{money.format(p.saldo_pendiente)}</td><td>{p.descripcion_estado_compra}</td>
            <td><button type="button" disabled={saving} onClick={() => setSelected(p)}>Ver detalle</button></td>
          </tr>)}</tbody>
        </table></div>}
      <nav className="purchase-pagination" aria-label="Páginas de compras">
        <button type="button" disabled={loading || saving || page === 1} onClick={() => { setSelected(null); setPage(p => p - 1) }}>Anterior</button>
        <span>Página {page}</span><button type="button" disabled={loading || saving || !!error || purchases.length < 20}
          onClick={() => { setSelected(null); setPage(p => p + 1) }}>Siguiente</button>
      </nav>
    </section>
    {selected && token && <PurchaseDetails key={selected.id_compra} purchase={selected} token={token}
      canPay={cashOpen && !loading && !error} onClose={() => setSelected(null)}
      onPaid={() => { setSelected(null); setRevision(v => v + 1) }} onUnauthorized={unauthorized} />}
  </div>
}
