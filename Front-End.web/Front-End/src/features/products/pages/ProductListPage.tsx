import { createPortal } from 'react-dom'
import '../../../shared/ui/catalog-row.css'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { getProductDetail, listProducts } from '../api/productQueries'
import type { ProductPage, ProductDetail } from '../types/product'
import { ProductDetailImage } from '../components/ProductDetailImage'
import { EditProductForm } from '../components/EditProductForm'
import './ProductsPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })

export function ProductListPage({ idMarca, idCategoria, productId, onProductSaved }: { idMarca?: number; idCategoria?: number; productId?: number; onProductSaved?: () => void } = {}) {
  const { session, logout } = useAuth()
  const [editing, setEditing] = useState(false)
  const [focusPresentations, setFocusPresentations] = useState(false)
  const [saving, setSaving] = useState(false)
  const [notice, setNotice] = useState('')
  const [revision, setRevision] = useState(0)
  const [data, setData] = useState<ProductPage | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [selected, setSelected] = useState<number | null>(null)
  const [detail, setDetail] = useState<ProductDetail | null>(null)
  const [detailLoading, setDetailLoading] = useState(false)
  const [detailError, setDetailError] = useState('')
  const [retryAt, setRetryAt] = useState(0)
  const [now, setNow] = useState(0)
  const detailPanel = useRef<HTMLDialogElement>(null)
  const trigger = useRef<HTMLButtonElement | null>(null)
  const token = session?.user.role === 'ADMINISTRADOR' ? session.token : undefined
  const waiting = Math.max(0, Math.ceil((retryAt - now) / 1000))

  const report = useCallback((failure: unknown, setMessage: (message: string) => void) => {
    if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
    else {
      if (failure instanceof ApiError && failure.status === 429) {
        const timestamp = Date.now()
        setNow(timestamp); setRetryAt(timestamp + (failure.retryAfterSeconds ?? 60) * 1000)
      }
      setMessage(failure instanceof ApiError ? failure.status === 404 ? 'El producto ya no está disponible.' : failure.message : 'No se pudieron cargar los productos.')
    }
  }, [logout])
  useEffect(() => {
    if (!retryAt) return
    const timer = window.setInterval(() => setNow(Date.now()), 1000)
    return () => window.clearInterval(timer)
  }, [retryAt])
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    void Promise.resolve().then(() => {
      if (controller.signal.aborted) return null
      setLoading(true); setError(''); setData(null)
      return productId !== undefined
        ? getProductDetail(productId, token, controller.signal).then(product => ({ pagina: 1, tamanoPagina: 20, total: 1, items: [product] }))
        : (async () => {
            const first = await listProducts(1, token, controller.signal, { idMarca, idCategoria })
            const items = [...first.items]
            const ids = new Set(items.map(item => item.idProducto))
            const lastPage = Math.ceil(first.total / first.tamanoPagina)
            for (let next = 2; next <= lastPage; next++) {
              const batch = await listProducts(next, token, controller.signal, { idMarca, idCategoria })
              for (const item of batch.items) {
                if (!ids.has(item.idProducto)) { ids.add(item.idProducto); items.push(item) }
              }
            }
            return { ...first, items }
          })()
    }).then(result => {
      if (result && !controller.signal.aborted) {
        setData(result)
      }
    }).catch(failure => { if (!controller.signal.aborted) report(failure, setError) })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, report, revision, idMarca, idCategoria, productId])
  useEffect(() => {
    if (!token || selected === null) return
    const controller = new AbortController()
    void Promise.resolve().then(() => {
      if (controller.signal.aborted) return null
      setDetail(null); setDetailError(''); setDetailLoading(true)
      return getProductDetail(selected, token, controller.signal)
    }).then(result => {
      if (!controller.signal.aborted) setDetail(result)
    }).catch(failure => { if (!controller.signal.aborted) report(failure, setDetailError) })
      .finally(() => { if (!controller.signal.aborted) setDetailLoading(false) })
    return () => controller.abort()
  }, [token, report, selected, revision])
  useEffect(() => {
    if (selected === null) return
    const dialog = detailPanel.current
    dialog?.showModal()
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      dialog?.close()
      document.body.style.overflow = previousOverflow
    }
  }, [selected])
  if (!token) return null
  const busy = loading || waiting > 0 || editing || saving
  function closeDetail() {
    if (saving) return
    setEditing(false); setSelected(null); setDetail(null); setDetailError('')
    requestAnimationFrame(() => trigger.current?.focus())
  }

  return <div className="product-browser">
    <section className="product-card product-list-card" aria-labelledby="product-list-title" aria-busy={loading}>
      <div className="product-list-heading">
        <h2 id="product-list-title">Productos</h2>
        <RefreshButton loading={busy} onClick={() => setRevision(value => value + 1)} label="Actualizar productos" />
      </div>
      {waiting > 0 && <p role="status">Espera {waiting} segundos antes de volver a consultar.</p>}
      {loading && <p role="status">Cargando productos…</p>}
      {error && <p role="alert" className="product-error">{error}</p>}
      {data && !data.items.length && <p>No hay productos registrados.</p>}
      {data && <>
        <div className="product-table-scroll">
          <table className="product-table">
            <thead><tr><th scope="col">Producto</th><th scope="col">Marca</th><th scope="col">Estado</th><th scope="col">Editar</th></tr></thead>
            <tbody>{data.items.map(product => <tr key={product.idProducto}>
              <td><button className="product-name-button" type="button" disabled={busy}
                aria-label={`Ver detalles de ${product.nombre}`} aria-expanded={selected === product.idProducto}
                aria-controls="product-detail"
                onClick={event => { trigger.current = event.currentTarget; setSelected(product.idProducto) }}>{product.nombre}</button></td>
              <td>{product.marca}</td>
              <td><span className={`product-stock-status ${product.stockUnidades <= 0 ? 'is-empty' : product.stockUnidades <= product.stockMinimo ? 'is-low' : 'is-available'}`}>
                {product.stockUnidades <= 0 ? 'Sin existencias' : product.stockUnidades <= product.stockMinimo ? 'Stock bajo' : 'Disponible'}
              </span></td>
              <td><button className="Product-update-button" type="button" disabled={busy}
                aria-label={`Editar ${product.nombre}`} aria-controls="product-detail"
                onClick={event => {
                  trigger.current = event.currentTarget
                  setNotice('')
                  if (selected !== product.idProducto) setDetail(null)
                  setSelected(product.idProducto)
                  setFocusPresentations(false); setEditing(true)
                }}>Editar</button></td>
            </tr>)}</tbody>
          </table>
        </div>
      </>}
    </section>
    {selected !== null && createPortal(<dialog id="product-detail" ref={detailPanel} tabIndex={-1} className="product-card product-detail-dialog"
      aria-labelledby="product-detail-title" aria-busy={detailLoading}
      onCancel={event => { event.preventDefault(); closeDetail() }}>
      <div className="product-detail-header"><h2 id="product-detail-title">Detalles del producto</h2>
        <div className="product-detail-header-actions">{detail && !editing && (<button className="Product-update-button" type="button" disabled={detailLoading || waiting > 0 || saving}
          aria-label={`Editar ${detail.nombre}`} title="Editar producto"
          onClick={() => { setNotice(''); setFocusPresentations(false); setEditing(true) }}>Editar
        </button>)}
        <button type="button" className="product-detail-close" aria-label="Cerrar detalles" title="Cerrar detalles" disabled={saving} onClick={closeDetail}>×</button></div></div>
      {detailLoading && <p role="status">Cargando detalles…</p>}
      {detailError && <p role="alert" className="product-error">{detailError}</p>}
      {detailError && <button type="button" disabled={waiting > 0} onClick={() => setRevision(value => value + 1)}>Reintentar</button>}
      {notice && <p role="status">{notice}</p>}
      {detail && editing && <EditProductForm key={detail.idProducto} focusPresentations={focusPresentations} product={detail} onPending={setSaving}
        onCancel={() => { setEditing(false); detailPanel.current?.focus() }}
        onSaved={() => { setEditing(false); setSaving(false); setNotice('Producto actualizado correctamente.'); onProductSaved?.(); setRevision(value => value + 1) }} />}
      {detail && !editing && <>
        <div className="product-detail-overview"><div className="product-detail-info"><h3>{detail.nombre}</h3>

        <dl className="product-detail-fields">
          <div><dt>Marca</dt><dd>{detail.marca}{!detail.marcaActiva && ' (inactiva)'}</dd></div>
          <div><dt>Categoría</dt><dd>{detail.categoria}{!detail.categoriaActiva && ' (inactiva)'}</dd></div>
          <div><dt>Código de barras</dt><dd>{detail.codigoBarra || 'Sin código'}</dd></div>
          <div><dt>Existencia en unidades</dt><dd>{detail.stockUnidades}</dd></div>
          <div><dt>Existencia minima</dt><dd>{detail.stockMinimo}</dd></div>
        </dl></div><ProductDetailImage key={`${detail.idProducto}:${detail.imagen ?? ""}`} image={detail.imagen} name={detail.nombre} /></div>
                <div className="product-list-heading">
          <h3>Presentaciones</h3>
          
        </div>
        {!detail.presentaciones.length && <p>Este producto no tiene presentaciones.</p>}
        {detail.presentaciones.length > 0 && <div className="product-presentations-table-scroll">
          <table className="product-presentations-table">
            <thead><tr><th scope="col">Presentación</th><th scope="col">Unidades</th><th scope="col">Precio</th><th scope="col">Disponibles</th><th scope="col">Estado</th></tr></thead>
            <tbody>{detail.presentaciones.map(item => <tr key={item.idProductoPresentacion}>
              <td><strong>{item.descripcion || 'Sin descripción'}</strong></td>
              <td>{item.unidadesEquivalentes}</td>
              <td>{money.format(item.precio)}</td>
              <td>{item.presentacionesDisponibles}</td>
              <td><span className={`product-stock-status ${item.activa && item.presentacionActiva ? 'is-available' : 'is-empty'}`}>{item.activa && item.presentacionActiva ? 'Activa' : 'Inactiva'}</span></td>
            </tr>)}</tbody>
          </table>
        </div>}
      </>}
    </dialog>, document.body)}
  </div>
}

