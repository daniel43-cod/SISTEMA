import { createPortal } from 'react-dom'
import { InventoryLookup } from '../components/InventoryLookup'
import { loadCatalogLookups } from '../../catalog-lookups/api/catalogLookupsApi'
import type { CatalogLookups } from '../../catalog-lookups/api/catalogLookupsApi'
import { clearCatalogLookupCache } from '../../../shared/cache/catalogLookupCache'
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
  const [categoryQuery, setCategoryQuery] = useState('')
  const [brandQuery, setBrandQuery] = useState('')
  const [categoryId, setCategoryId] = useState<number>()
  const [brandId, setBrandId] = useState<number>()
  const [lookups, setLookups] = useState<CatalogLookups>({ categories: [], brands: [] })
  const [lookupLoading, setLookupLoading] = useState(true)
  const [lookupError, setLookupError] = useState('')
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
    let active = true
    async function load() {
      setLookupLoading(true); setLookupError('')
      try {
        const result = await loadCatalogLookups(token!)
        if (active) setLookups(result)
      } catch (failure) {
        if (!active) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setLookupError(failure instanceof Error ? failure.message : 'No se pudieron cargar categorías y marcas.')
      } finally { if (active) setLookupLoading(false) }
    }
    void load()
    return () => { active = false }
  }, [token, allowed, revision, logout])

  useEffect(() => {
    if (!token || !allowed) return
    const controller = new AbortController()
    setLoading(true); setError(''); setData(null)
    void listInventory(page, text, token, controller.signal, { idCategoria: categoryId, idMarca: brandId }).then(result => {
      if (!controller.signal.aborted) setData(result)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof Error ? failure.message : 'No se pudo cargar el inventario.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, allowed, page, text, categoryId, brandId, revision, logout])

  function activateSearch(mode: 'product' | 'category' | 'brand') {
    setPage(1)
    if (mode !== 'product') { setQuery(''); setText(''); setSuggestionsOpen(false) }
    if (mode !== 'category') { setCategoryId(undefined); setCategoryQuery('') }
    if (mode !== 'brand') { setBrandId(undefined); setBrandQuery('') }
  }

  if (!allowed) return null
  const pages = Math.max(1, Math.ceil((data?.total ?? 0) / 25))
  const headerTarget = document.getElementById('staff-inventory-search')
  const toolbar = (
<div className="inventory-toolbar">
      <div className="inventory-search" onBlur={event => {
        if (!event.currentTarget.contains(event.relatedTarget)) setSuggestionsOpen(false)
      }}>
      <label htmlFor="inventory-search" >Producto</label>
        <input id="inventory-search" type="search" maxLength={100} value={query}
          placeholder="Buscar producto…" aria-expanded={suggestionsOpen && query.trim().length >= 3}
          aria-controls="inventory-suggestions" onFocus={() => setSuggestionsOpen(true)}
          onKeyDown={event => {
            if (event.key === 'Escape') setSuggestionsOpen(false)
            if (event.key === 'Enter') { activateSearch('product'); setText(query.trim()); setPage(1); setSuggestionsOpen(false) }
          }}
          onChange={event => {
            const value = event.target.value
            activateSearch('product'); setQuery(value); setSuggestionsOpen(true)
            if (!value.trim()) { setText(''); setPage(1) }
          }} />
      {suggestionsOpen && query.trim().length >= 3 && <div id="inventory-suggestions" className="inventory-suggestions">
        {names.loading ? <p role="status">Cargando productos…</p>
          : names.error ? <p role="alert">{names.error}</p>
          : suggestions.length ? <ul aria-label="Productos sugeridos">{suggestions.map(item => <li key={item.idProducto}>
            <button type="button" onClick={() => {
              activateSearch('product'); setQuery(item.nombre); setText(item.nombre); setPage(1); setSuggestionsOpen(false)
            }}>{item.nombre}</button>
          </li>)}</ul> : <p role="status">No se encontraron productos.</p>}
      </div>}
      </div>
      <InventoryLookup label="Categoría" items={lookups.categories.map(item => ({ id: item.idCategoria, nombre: item.nombre }))}
        query={categoryQuery} onQueryChange={value => { activateSearch('category'); setCategoryQuery(value) }}
        selected={categoryId} loading={lookupLoading} error={lookupError}
        onSelect={id => { setCategoryId(id); setPage(1) }} />
      <InventoryLookup label="Marca" items={lookups.brands.map(item => ({ id: item.idMarca, nombre: item.nombre }))}
        query={brandQuery} onQueryChange={value => { activateSearch('brand'); setBrandQuery(value) }}
        selected={brandId} loading={lookupLoading} error={lookupError}
        onSelect={id => { setBrandId(id); setPage(1) }} />
      <RefreshButton loading={loading || names.loading || lookupLoading} label="Actualizar inventario"
        onClick={() => { clearCatalogLookupCache(); setRevision(value => value + 1) }} />
    </div>
  )
  return <section className="inventory-page" aria-label="Consulta de inventario" aria-busy={loading}>
    {headerTarget ? createPortal(toolbar, headerTarget) : toolbar}
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
