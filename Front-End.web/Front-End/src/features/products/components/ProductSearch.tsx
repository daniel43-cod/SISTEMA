import { useState } from 'react'
import { useAuth } from '../../auth'
import { filterProductNames } from '../api/searchProducts'
import { useProductNames } from '../hooks/useProductNames'

import { ProductListPage } from '../pages/ProductListPage'

export function ProductSearch({ onResult, namesRevision, onNamesRefresh, onBack, backLabel }: {
  onResult: (active: boolean) => void; namesRevision: number; onNamesRefresh: () => void; onBack?: () => void; backLabel?: string
}) {
  const { session } = useAuth()
  const [text, setText] = useState('')
  const [selected, setSelected] = useState<number | null>(null)
  const names = useProductNames(namesRevision)
  const suggestions = filterProductNames(names.items, text)
  function changeText(value: string) { setText(value); setSelected(null); onResult(false) }
  if (session?.user.role !== 'ADMINISTRADOR') return null
  return <section className="product-search product-search-compact" aria-label="Buscar productos por nombre">
    <div className="product-search-bar">
      {(selected !== null || onBack) && <button type="button" className="product-back-icon"
        aria-label={selected !== null ? 'Volver al catálogo' : backLabel} title={selected !== null ? 'Volver al catálogo' : backLabel}
        onClick={() => { if (selected !== null) changeText(''); else onBack?.() }}>
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m12 19-7-7 7-7M5 12h14" /></svg>
      </button>}
      <div className="product-search-input">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
          <circle cx="10.5" cy="10.5" r="6.5" /><path d="m16 16 4 4" />
        </svg>
        <input type="search" aria-label="Buscar producto por nombre" value={text} maxLength={200}
          placeholder="Buscar producto…" onChange={event => changeText(event.target.value)}
          onKeyDown={event => { if (event.key === 'Escape') changeText('') }} />
        {text && <button type="button" className="product-search-clear" aria-label="Limpiar búsqueda" title="Limpiar búsqueda" onClick={() => changeText('')}>×</button>}
      </div>

    </div>
    {names.loading && <p role="status">Cargando nombres de productos…</p>}
    {names.error && <><p role="alert" className="product-error">{names.error}</p><button type="button" onClick={onNamesRefresh}>Reintentar</button></>}
    {text.trim().length >= 3 && !names.loading && !names.error && !selected && !suggestions.length && <p role="status">No se encontraron productos.</p>}
    {suggestions.length > 0 && !selected && <ul className="product-search-suggestions" aria-label="Productos sugeridos">
      {suggestions.map(item => <li key={item.idProducto}><button type="button" onClick={() => { setSelected(item.idProducto); onResult(true) }}>{item.nombre}</button></li>)}
    </ul>}
    {selected !== null && <ProductListPage key={selected} productId={selected} onProductSaved={onNamesRefresh} />}
  </section>
}