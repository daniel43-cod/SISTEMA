import { useId, useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import type { KeyboardEvent } from 'react'
import { filterProductNames } from '../../products/api/searchProducts'
import type { ProductSuggestion } from '../../products/api/searchProducts'
import type { PurchaseLine } from '../types/purchase'
import { amountCents } from '../utils/purchaseValidation'

export function PurchaseProductRow({ line, products, usedIds, onChange, onNext, onRemove, autoFocus }: {
  line: PurchaseLine; products: ProductSuggestion[]; usedIds: number[]
  onChange: (line: PurchaseLine) => void; onNext: () => void; onRemove: () => void; autoFocus: boolean
}) {
  const [open, setOpen] = useState(false)
  const optionsId = useId()
  const [active, setActive] = useState(0)
  const searchInput = useRef<HTMLInputElement>(null)
  const dropdown = useRef<HTMLUListElement>(null)
  const quantity = useRef<HTMLInputElement>(null)
  const total = useRef<HTMLInputElement>(null)
  const suggestions = filterProductNames(products.filter(item => !usedIds.includes(item.idProducto)), line.nombre)
  useLayoutEffect(() => {
    if (!open || line.idProducto || line.nombre.trim().length < 3) return
    function position() {
      const input = searchInput.current
      const list = dropdown.current
      if (!input || !list) return
      const rect = input.getBoundingClientRect()
      const below = window.innerHeight - rect.bottom - 12
      const above = rect.top - 12
      const upwards = below < 240 && above > below
      list.style.left = `${Math.max(8, Math.min(rect.left, window.innerWidth - rect.width - 8))}px`
      list.style.width = `${Math.min(rect.width, window.innerWidth - 16)}px`
      list.style.maxHeight = `${Math.max(44, Math.min(240, upwards ? above : below))}px`
      list.style.top = upwards ? 'auto' : `${rect.bottom + 6}px`
      list.style.bottom = upwards ? `${window.innerHeight - rect.top + 6}px` : 'auto'
    }
    position()
    dropdown.current?.querySelector('[aria-selected="true"]')?.scrollIntoView({ block: 'nearest' })
    window.addEventListener('resize', position)
    window.addEventListener('scroll', position, true)
    return () => { window.removeEventListener('resize', position); window.removeEventListener('scroll', position, true) }
  }, [open, line.idProducto, line.nombre, active])
  function choose(item: ProductSuggestion) {
    onChange({ ...line, idProducto: item.idProducto, nombre: item.nombre })
    setOpen(false)
    quantity.current?.focus()
  }
  function advance(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key !== 'Enter' || event.nativeEvent.isComposing) return
    event.preventDefault()
    const validQuantity = /^\d+$/.test(line.cantidad) && Number(line.cantidad) > 0 && Number(line.cantidad) <= 2147483647
    if (!validQuantity) { quantity.current?.focus(); return }
    if ((amountCents(line.total) ?? 0) <= 0) { total.current?.focus(); return }
    if (line.idProducto > 0) onNext()
  }
  return <tr>
    <td className="purchase-product-cell">
      <input ref={searchInput} aria-label="Producto" role="combobox" aria-autocomplete="list" aria-expanded={open && suggestions.length > 0}
        aria-activedescendant={open && suggestions.length ? `${optionsId}-${Math.min(active, suggestions.length - 1)}` : undefined}
        aria-controls={optionsId} autoComplete="off" autoFocus={autoFocus}
        placeholder="Escribe al menos 3 letras" value={line.nombre} maxLength={200}
        onFocus={() => { if (!line.idProducto) setOpen(true) }}
        onBlur={event => { if (!event.currentTarget.parentElement?.contains(event.relatedTarget)) setOpen(false) }}
        onChange={event => { onChange({ ...line, idProducto: 0, nombre: event.target.value }); setOpen(true); setActive(0) }}
        onKeyDown={event => {
          if (event.nativeEvent.isComposing) return
          if (event.key === 'Escape') { setOpen(false); return }
          if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault(); setOpen(true)
            setActive(value => suggestions.length ? (value + (event.key === 'ArrowDown' ? 1 : -1) + suggestions.length) % suggestions.length : 0)
          }
          if (event.key === 'Enter') {
            event.preventDefault()
            if (open && suggestions.length) choose(suggestions[Math.min(active, suggestions.length - 1)])
            else if (line.idProducto) quantity.current?.focus()
          }
        }} />
      {open && !line.idProducto && line.nombre.trim().length >= 3 && createPortal(<ul ref={dropdown} className="purchase-suggestions purchase-floating-suggestions" id={optionsId} role="listbox" aria-label="Productos coincidentes">
        {suggestions.map((item, index) => <li key={item.idProducto} id={`${optionsId}-${index}`} role="option" aria-selected={index === active}><button type="button" tabIndex={-1}
          className={index === active ? 'is-active' : ''}
          onMouseDown={event => event.preventDefault()} onClick={() => choose(item)}>{item.nombre}</button></li>)}
        {!suggestions.length && <li role="status">No hay productos disponibles que coincidan.</li>}
      </ul>, document.body)}
    </td>
    <td><input ref={quantity} aria-label={`Unidades de ${line.nombre || 'producto'}`} inputMode="numeric"
      disabled={!line.idProducto} value={line.cantidad} onChange={event => onChange({ ...line, cantidad: event.target.value })}
      onKeyDown={advance} required={line.idProducto > 0} /></td>
    <td><input ref={total} aria-label={`Total de ${line.nombre || 'producto'}`} inputMode="decimal"
      disabled={!line.idProducto} value={line.total} onChange={event => onChange({ ...line, total: event.target.value })}
      onKeyDown={advance} required={line.idProducto > 0} /></td>
    <td><button type="button" className="purchase-remove-line" aria-label={`Quitar ${line.nombre || 'fila'}`}
      title="Quitar producto" onClick={onRemove}>
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M3 6h18M9 6V4h6v2M5 6l1 14h12l1-14M10 10v6M14 10v6" />
      </svg>
    </button></td>
  </tr>
}
