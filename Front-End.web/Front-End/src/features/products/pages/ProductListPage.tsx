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

export function ProductListPage() {
  const { session, logout } = useAuth()
  const [editing, setEditing] = useState(false)
  const [saving, setSaving] = useState(false)
  const [notice, setNotice] = useState('')
  const [page, setPage] = useState(1)
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
  const detailPanel = useRef<HTMLElement>(null)
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
      return listProducts(page, token, controller.signal)
    }).then(result => {
      if (result && !controller.signal.aborted) {
        const lastPage = Math.max(1, Math.ceil(result.total / result.tamanoPagina))
        if (page > lastPage) setPage(lastPage)
        else setData(result)
      }
    }).catch(failure => { if (!controller.signal.aborted) report(failure, setError) })
      .finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, report, page, revision])
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
  useEffect(() => { if (selected !== null) detailPanel.current?.focus() }, [selected])
  if (!token) return null
  const pages = data ? Math.max(1, Math.ceil(data.total / data.tamanoPagina)) : 1
  const busy = loading || waiting > 0 || editing || saving
  function closeDetail() {
    if (saving) return
    setEditing(false); setSelected(null); setDetail(null); setDetailError('')
    requestAnimationFrame(() => trigger.current?.focus())
  }

  return <div className="product-browser">
    <section className="product-card" aria-labelledby="product-list-title" aria-busy={loading}>
      <div className="product-list-heading">
        <h2 id="product-list-title">Productos</h2>
        <RefreshButton loading={busy} onClick={() => setRevision(value => value + 1)} label="Actualizar productos" />
      </div>
      {waiting > 0 && <p role="status">Espera {waiting} segundos antes de volver a consultar.</p>}
      {loading && <p role="status">Cargando productos…</p>}
      {error && <p role="alert" className="product-error">{error}</p>}
      {data && !data.items.length && <p>No hay productos registrados.</p>}
      {data && <>
        <ul className="product-list">
          {data.items.map(product => <li key={product.idProducto}>
            <div><strong>{product.nombre}</strong><span>Marca: {product.marca}</span></div>
                        <button className="Product-update-button" type="button" disabled={busy}
              aria-label={`Editar ${product.nombre}`} title="Editar producto"
              aria-controls="product-detail"
              onClick={event => {
                trigger.current = event.currentTarget
                setNotice('')
                if (selected !== product.idProducto) setDetail(null)
                setSelected(product.idProducto)
                setEditing(true)
              }}>
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"
                  strokeLinejoin="round" aria-hidden="true">
                  <path d="m16 3 5 5" />
                  <path d="M4 16 16.5 3.5a3.54 3.54 0 0 1 5 5L9 21l-6 1 1-6Z" />
                </svg>
            </button>
            <button className="product-detail-button" type="button" disabled={busy} aria-label={`Ver detalles de ${product.nombre}`}
              aria-expanded={selected === product.idProducto} aria-controls="product-detail" title='Ver detalles'
              onClick={event => { trigger.current = event.currentTarget; setSelected(product.idProducto) }}>
               <svg
               width="20"
               height="20" viewBox="0 0 24 24"fill="none"stroke="currentColor"strokeWidth="1.8"strokeLinecap="round"
               strokeLinejoin="round"aria-hidden="true"
               >
                <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" />
                <circle cx="12" cy="12" r="3" />
               </svg>
              </button>
             
          </li>)}
        </ul>
        <nav className="product-pagination" aria-label="Páginas de productos">
          <button type="button" disabled={busy || page <= 1} onClick={() => { closeDetail(); setPage(value => value - 1) }}>Anterior</button>
          <span>Página {page} de {pages}</span>
          <button type="button" disabled={busy || page >= pages || page >= 100000} onClick={() => { closeDetail(); setPage(value => value + 1) }}>Siguiente</button>
        </nav>
      </>}
    </section>
    {selected !== null && <section id="product-detail" ref={detailPanel} tabIndex={-1} className="product-card"
      aria-labelledby="product-detail-title" aria-busy={detailLoading}
      onKeyDown={event => { if (event.key === 'Escape') closeDetail() }}>
      <div className="product-list-heading"><h2 id="product-detail-title">Detalles del producto</h2>
        <button type="button" disabled={saving} onClick={closeDetail}>Cerrar detalles</button></div>
      {detailLoading && <p role="status">Cargando detalles…</p>}
      {detailError && <p role="alert" className="product-error">{detailError}</p>}
      {detailError && <button type="button" disabled={waiting > 0} onClick={() => setRevision(value => value + 1)}>Reintentar</button>}
      {notice && <p role="status">{notice}</p>}
      {detail && editing && <EditProductForm key={detail.idProducto} product={detail} onPending={setSaving}
        onCancel={() => { setEditing(false); detailPanel.current?.focus() }}
        onSaved={() => { setEditing(false); setSaving(false); setNotice('Producto actualizado correctamente.'); setRevision(value => value + 1) }} />}
      {detail && !editing && <>
                   <button className="Product-update-button" type="button" disabled={detailLoading || waiting > 0 || saving}
          aria-label={`Editar ${detail.nombre}`} title="Editar producto"
          onClick={() => { setNotice(''); setEditing(true) }}>
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none"
                  stroke="currentColor" strokeWidth="1.8" strokeLinecap="round"
                  strokeLinejoin="round" aria-hidden="true">
                  <path d="m16 3 5 5" />
                  <path d="M4 16 16.5 3.5a3.54 3.54 0 0 1 5 5L9 21l-6 1 1-6Z" />
                </svg>
        </button>
        <h3>{detail.nombre}</h3>
        <ProductDetailImage key={`${detail.idProducto}:${detail.imagen ?? ""}`} image={detail.imagen} name={detail.nombre} />
        <dl className="product-detail-fields">
          <div><dt>Marca</dt><dd>{detail.marca}{!detail.marcaActiva && ' (inactiva)'}</dd></div>
          <div><dt>Categoría</dt><dd>{detail.categoria}{!detail.categoriaActiva && ' (inactiva)'}</dd></div>
          <div><dt>Código de barras</dt><dd>{detail.codigoBarra || 'Sin código'}</dd></div>
          <div><dt>Existencia en unidades</dt><dd>{detail.stockUnidades}</dd></div>
          <div><dt>Existencia minima</dt><dd>{detail.stockMinimo}</dd></div>
        </dl>
        <h3>Presentaciones</h3>
        {!detail.presentaciones.length && <p>Este producto no tiene presentaciones.</p>}
        <ul className="product-detail-presentations">
          {detail.presentaciones.map(item => <li key={item.idProductoPresentacion}>
            <strong>{item.descripcion || 'Sin descripción'}</strong>
            <span>{item.unidadesEquivalentes} unidades · {money.format(item.precio)}</span>
            <span>{item.activa && item.presentacionActiva ? 'Activa' : 'Inactiva'} · Disponibles: {item.presentacionesDisponibles}</span>
          </li>)}
        </ul>
      </>}
    </section>}
  </div>
}