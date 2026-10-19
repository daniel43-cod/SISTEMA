import { createPortal } from 'react-dom'
import { useId, useRef, useState } from 'react'
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
  const [activeId, setActiveId] = useState<number | null>(null)
  const listId = useId()
  const listRef = useRef<HTMLUListElement>(null)
  const names = useProductNames(namesRevision)
  const suggestions = filterProductNames(names.items, text)
  function changeText(value: string) { setText(value); setActiveId(null); setSelected(null); onResult(false) }
  const expanded = selected === null && suggestions.length > 0
  const activeIndex = suggestions.findIndex(item => item.idProducto === activeId)
  function chooseProduct(id: number) { setSelected(id); setActiveId(null); onResult(true) }
  if (session?.user.role !== 'ADMINISTRADOR') return null
  const headerTarget = document.getElementById('staff-product-search')
  const search = <section className="product-search product-search-compact" aria-label="Buscar productos por nombre">
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
          role="combobox" aria-autocomplete="list" aria-expanded={expanded} aria-controls={expanded ? listId : undefined}
          aria-activedescendant={expanded && activeIndex >= 0 ? `${listId}-${activeId}` : undefined}
          placeholder="Buscar producto…" onChange={event => changeText(event.target.value)}
          onKeyDown={event => {
            if (event.nativeEvent.isComposing) return
            if (event.key === 'Escape') { event.preventDefault(); changeText(''); return }
            if (!expanded) return
            if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
              event.preventDefault()
              const next = activeIndex < 0 ? (event.key === 'ArrowDown' ? 0 : suggestions.length - 1)
                : (activeIndex + (event.key === 'ArrowDown' ? 1 : -1) + suggestions.length) % suggestions.length
              setActiveId(suggestions[next].idProducto)
              requestAnimationFrame(() => listRef.current?.children[next]?.scrollIntoView({ block: 'nearest' }))
            } else if (event.key === 'Enter') {
              event.preventDefault()
              chooseProduct(suggestions[activeIndex >= 0 ? activeIndex : 0].idProducto)
            }
          }} />
        {text && <button type="button" className="product-search-clear" aria-label="Limpiar búsqueda" title="Limpiar búsqueda" onClick={() => changeText('')}>×</button>}
      </div>

    </div>
    {names.loading && <p role="status">Cargando nombres de productos…</p>}
    {names.error && <><p role="alert" className="product-error">{names.error}</p><button type="button" onClick={onNamesRefresh}>Reintentar</button></>}
    {text.trim().length >= 3 && !names.loading && !names.error && !selected && !suggestions.length && <p role="status">No se encontraron productos.</p>}
    {suggestions.length > 0 && !selected && <ul ref={listRef} id={listId} role="listbox" className="product-search-suggestions" aria-label="Productos sugeridos">
      {suggestions.map(item => <li key={item.idProducto} id={`${listId}-${item.idProducto}`} role="option" aria-selected={activeId === item.idProducto} onMouseEnter={() => setActiveId(item.idProducto)} onMouseDown={event => event.preventDefault()} onClick={() => chooseProduct(item.idProducto)}>{item.nombre}</li>)}
    </ul>}

  </section>
  return <>
    {headerTarget ? createPortal(search, headerTarget) : search}
    {selected !== null && <ProductListPage key={selected} productId={selected} onProductSaved={onNamesRefresh} />}
  </>
}