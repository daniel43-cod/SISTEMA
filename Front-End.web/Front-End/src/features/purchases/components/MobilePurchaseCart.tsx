import { useRef, useState } from 'react'
import { filterProductNames } from '../../products/api/searchProducts'
import type { ProductSuggestion } from '../../products/api/searchProducts'
import type { PurchaseLine } from '../types/purchase'
import { amountCents } from '../utils/purchaseValidation'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function MobilePurchaseCart({ products, lines, onChange, onSupplier, total, paid, onPaid, saving, onClose }: {
  products: ProductSuggestion[]; lines: PurchaseLine[]; onChange: (lines: PurchaseLine[]) => void
  onSupplier: () => void; onClose: () => void; total: number; paid: string; onPaid: (value: string) => void; saving: boolean
}) {
  const [query, setQuery] = useState('')
  const [active, setActive] = useState(0)
  const [draft, setDraft] = useState<PurchaseLine | null>(null)
  const [error, setError] = useState('')
  const search = useRef<HTMLInputElement>(null)
  const selected = lines.filter(line => line.idProducto > 0)
  const options = filterProductNames(products.filter(p => !selected.some(line => line.idProducto === p.idProducto)), query)
  function choose(product: ProductSuggestion) {
    setDraft({ idProducto: product.idProducto, nombre: product.nombre, cantidad: '', total: '' })
    setQuery(''); setError('')
  }
  function finish() {
    if (!draft) return
    if (!/^\d+$/.test(draft.cantidad) || Number(draft.cantidad) < 1 || Number(draft.cantidad) > 2147483647 ||
      (amountCents(draft.total) ?? 0) <= 0) { setError('Ingresa unidades válidas y el total de la línea.'); return }
    const next = selected.some(line => line.idProducto === draft.idProducto)
      ? selected.map(line => line.idProducto === draft.idProducto ? draft : line) : [...selected, draft]
    onChange(next); setDraft(null); requestAnimationFrame(() => search.current?.focus())
  }
  function quantity(line: PurchaseLine, delta: number) {
    const value = Number(line.cantidad) + delta
    if (!Number.isSafeInteger(value) || value < 1 || value > 2147483647) return
    // El total de línea sigue siendo el importe ingresado, independiente de las unidades.
    onChange(selected.map(row => row.idProducto === line.idProducto ? { ...row, cantidad: String(value) } : row))
  }
  return <div className="purchase-mobile-cart">
    <div className="purchase-cart-top">
      <button type="button" onClick={onSupplier}>Proveedor</button>
      <p>Total<strong>{money.format(total / 100)}</strong></p>
      <button type="submit" className="purchase-primary" disabled={!selected.length || saving}>{saving ? 'Registrando…' : 'Registrar'}</button>
      <button type="button" className="purchase-cart-close" aria-label="Cerrar formulario" title="Cerrar formulario" disabled={saving} onClick={onClose}>×</button>
    </div>
    <div className="purchase-cart-search">
      <input ref={search} aria-label="Buscar producto" placeholder="Buscar producto" value={query}
        role="combobox" aria-expanded={options.length > 0} aria-controls="purchase-cart-options"
        aria-activedescendant={options.length ? 'purchase-cart-option-' + active : undefined}
        onChange={e => { setQuery(e.target.value); setActive(0) }}
        onKeyDown={e => {
          if (e.nativeEvent.isComposing) return
          if (e.key === 'Enter') { e.preventDefault(); if (options[active]) choose(options[active]) }
          if (e.key === 'Escape') setQuery('')
          if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
            e.preventDefault(); setActive(v => options.length ? (v + (e.key === 'ArrowDown' ? 1 : -1) + options.length) % options.length : 0)
          }
        }} />
      {query.trim().length >= 3 && <ul id="purchase-cart-options" role="listbox">
        {options.map((p, i) => <li key={p.idProducto} id={'purchase-cart-option-' + i} role="option" aria-selected={i === active}>
          <button type="button" onClick={() => choose(p)}>{p.nombre}</button>
        </li>)}
        {!options.length && <li>No hay productos disponibles que coincidan.</li>}
      </ul>}
    </div>
    <h3>Carrito ({selected.length})</h3>
    {selected.map(line => <article className="purchase-cart-card" key={line.idProducto}>
      <div><strong>{line.nombre}</strong><strong>{money.format((amountCents(line.total) ?? 0) / 100)}</strong></div>
      <div className="purchase-cart-controls">
        <button type="button" aria-label={'Reducir unidades de ' + line.nombre} onClick={() => quantity(line, -1)}>−</button>
        <strong>{line.cantidad}</strong>
        <button type="button" aria-label={'Aumentar unidades de ' + line.nombre} onClick={() => quantity(line, 1)}>+</button>
        <button type="button" onClick={() => { setDraft({ ...line }); setError('') }}>Editar</button>
        <button type="button" aria-label={'Quitar ' + line.nombre} onClick={() => onChange(selected.filter(p => p.idProducto !== line.idProducto))}>
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" aria-hidden="true"><path d="M3 6h18M9 6V4h6v2M5 6l1 14h12l1-14M10 10v6M14 10v6" /></svg>
        </button>
      </div>
    </article>)}
    <label className="purchase-cart-payment">Pago inicial (Q)<input inputMode="decimal" value={paid} onChange={e => onPaid(e.target.value)} /></label>
    <p>Saldo pendiente: {money.format(Math.max(0, total - (amountCents(paid) ?? 0)) / 100)}</p>
    {draft && <div className="purchase-cart-overlay">
      <section role="dialog" aria-modal="true" aria-labelledby="purchase-cart-editor-title" onKeyDown={e => {
        if (e.key === 'Escape') { e.preventDefault(); setDraft(null) }
        if (e.key === 'Enter') { e.preventDefault(); finish() }
        if (e.key === 'Tab') {
          const controls = Array.from(e.currentTarget.querySelectorAll<HTMLElement>('input, button'))
          const first = controls[0], last = controls[controls.length - 1]
          if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus() }
          else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus() }
        }
      }}>
        <h3 id="purchase-cart-editor-title">{draft.nombre}</h3>
        <label>Unidades recibidas<input autoFocus inputMode="numeric" value={draft.cantidad} onChange={e => setDraft({ ...draft, cantidad: e.target.value })} /></label>
        <label>Total de línea (Q)<input inputMode="decimal" value={draft.total} onChange={e => setDraft({ ...draft, total: e.target.value })} /></label>
        {error && <p role="alert">{error}</p>}
        <div><button type="button" onClick={() => setDraft(null)}>Cancelar</button><button type="button" onClick={finish}>Listo</button></div>
      </section>
    </div>}
  </div>
}