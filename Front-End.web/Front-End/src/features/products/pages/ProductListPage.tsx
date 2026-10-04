import { useCallback, useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { getProductDetail, listProducts } from '../api/productQueries'
import type { ProductPage, ProductDetail } from '../types/product'
import './ProductsPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })

export function ProductListPage() {
  const { session, logout } = useAuth()
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
  const busy = loading || waiting > 0
  function closeDetail() {
    setSelected(null); setDetail(null); setDetailError('')
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
            <button type="button" disabled={busy} aria-label={`Ver detalles de ${product.nombre}`}
              aria-expanded={selected === product.idProducto} aria-controls="product-detail"
              onClick={event => { trigger.current = event.currentTarget; setSelected(product.idProducto) }}>Detalles</button>
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
        <button type="button" onClick={closeDetail}>Cerrar detalles</button></div>
      {detailLoading && <p role="status">Cargando detalles…</p>}
      {detailError && <p role="alert" className="product-error">{detailError}</p>}
      {detailError && <button type="button" disabled={waiting > 0} onClick={() => setRevision(value => value + 1)}>Reintentar</button>}
      {detail && <>
        <h3>{detail.nombre}</h3>
        <dl className="product-detail-fields">
          <div><dt>Marca</dt><dd>{detail.marca}{!detail.marcaActiva && ' (inactiva)'}</dd></div>
          <div><dt>Categoría</dt><dd>{detail.categoria}{!detail.categoriaActiva && ' (inactiva)'}</dd></div>
          <div><dt>Código de barras</dt><dd>{detail.codigoBarra || 'Sin código'}</dd></div>
          <div><dt>Stock en unidades</dt><dd>{detail.stockUnidades}</dd></div>
          <div><dt>Stock mínimo</dt><dd>{detail.stockMinimo}</dd></div>
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