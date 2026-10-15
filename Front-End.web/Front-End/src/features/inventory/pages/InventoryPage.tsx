import { useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { useProductNames } from '../../products/hooks/useProductNames'
import { filterProductNames } from '../../products/api/searchProducts'
import { listInventory } from '../api/inventoryApi'
import type { InventoryPageData } from '../api/inventoryApi'
import './InventoryPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ', maximumFractionDigits: 6 })

export function InventoryPage() {
  const { session, logout } = useAuth()
  const [data, setData] = useState<InventoryPageData | null>(null)
  const [page, setPage] = useState(1)
  const [query, setQuery] = useState('')
  const [text, setText] = useState('')
  const [revision, setRevision] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [suggestionsOpen, setSuggestionsOpen] = useState(false)
  const names = useProductNames(revision)
  const suggestions = filterProductNames(names.items, query)
  const token = session?.token
  const allowed = session?.user.role === 'ADMINISTRADOR'

  useEffect(() => {
    if (!token || !allowed) return
    const controller = new AbortController()
    setLoading(true); setError(''); setData(null)
    void listInventory(page, text, token, controller.signal).then(result => {
      if (!controller.signal.aborted) setData(result)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof Error ? failure.message : 'No se pudo cargar el inventario.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, allowed, page, text, revision, logout])

  if (!allowed) return null
  const pages = Math.max(1, Math.ceil((data?.total ?? 0) / 25))
  return <section className="inventory-page" aria-label="Consulta de inventario" aria-busy={loading}>
    <div className="inventory-toolbar">
      <div className="inventory-search" onBlur={event => {
        if (!event.currentTarget.contains(event.relatedTarget)) setSuggestionsOpen(false)
      }}>
      <label htmlFor="inventory-search" className="sr-only">Buscar producto por nombre</label>
        <input id="inventory-search" type="search" maxLength={100} value={query}
          placeholder="Buscar producto…" aria-expanded={suggestionsOpen && query.trim().length >= 3}
          aria-controls="inventory-suggestions" onFocus={() => setSuggestionsOpen(true)}
          onKeyDown={event => { if (event.key === 'Escape') setSuggestionsOpen(false) }}
          onChange={event => {
            const value = event.target.value
            setQuery(value); setSuggestionsOpen(true)
            if (!value.trim()) { setText(''); setPage(1) }
          }} />
      {suggestionsOpen && query.trim().length >= 3 && <div id="inventory-suggestions" className="inventory-suggestions">
        {names.loading ? <p role="status">Cargando productos…</p>
          : names.error ? <p role="alert">{names.error}</p>
          : suggestions.length ? <ul aria-label="Productos sugeridos">{suggestions.map(item => <li key={item.idProducto}>
            <button type="button" onClick={() => {
              setQuery(item.nombre); setText(item.nombre); setPage(1); setSuggestionsOpen(false)
            }}>{item.nombre}</button>
          </li>)}</ul> : <p role="status">No se encontraron productos.</p>}
      </div>}
      </div>
      <RefreshButton loading={loading || names.loading} label="Actualizar inventario"
        onClick={() => setRevision(value => value + 1)} />
    </div>
    {loading ? <p role="status">Cargando inventario…</p> : error ? <div role="alert">
      <p>{error}</p><button type="button" onClick={() => setRevision(value => value + 1)}>Reintentar</button>
    </div> : data && <>
      <p role="status">{data.total} productos{text ? ' encontrados' : ' en inventario'}</p>
      {data.items.length ? <div className="inventory-table-scroll" role="region" aria-label="Tabla de inventario" tabIndex={0}>
        <table>
          <caption className="sr-only">Existencias y precio promedio de compra por producto</caption>
          <thead><tr><th scope="col">Código de barras</th><th scope="col">Producto</th><th scope="col">Marca</th>
            <th scope="col" className="inventory-number">Existencia</th><th scope="col" className="inventory-number">Precio Costo</th></tr></thead>
          <tbody>{data.items.map(item => <tr key={item.idProducto}>
            <td className="inventory-barcode">{item.codigoBarra || 'Sin código'}</td><td>{item.nombre}</td><td>{item.marca}</td>
            <td className="inventory-number">{item.stock.toLocaleString('es-GT')}</td>
            <td className="inventory-number">{item.precioPromedioCompra === null ? 'Sin registro' : money.format(item.precioPromedioCompra)}</td>
          </tr>)}</tbody>
        </table>
      </div> : <p>No hay productos{ text ? ' que coincidan con la búsqueda' : ' en esta página'}.</p>}
      <nav className="inventory-pagination" aria-label="Páginas de inventario">
        <button type="button" disabled={page <= 1} onClick={() => setPage(value => value - 1)}>Anterior</button>
        <span>Página {page} de {pages}</span>
        <button type="button" disabled={page >= pages || page >= 100000} onClick={() => setPage(value => value + 1)}>Siguiente</button>
      </nav>
    </>}
  </section>
}
