import { useState } from 'react'
import type { PublicSurtido, SurtidoCatalogSelection } from '../types/product'
import { surtidoCatalogSelections } from '../utils/surtidoSelection'
import type { SurtidoCatalogInputs } from '../utils/surtidoSelection'
import { CatalogImage } from './CatalogImage'
import './CatalogSurtidoCard.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function CatalogSurtidoCard({ product, actionLabel, onAdd, initial = [], disabled = false, lockedIds = [] }: {
  product: PublicSurtido; actionLabel: string; disabled?: boolean; lockedIds?: number[]; initial?: SurtidoCatalogSelection[]
  onAdd: (selections: SurtidoCatalogSelection[]) => string | null
}) {
  const [values, setValues] = useState<SurtidoCatalogInputs>(() => Object.fromEntries(initial.map(s => [s.idProductoPresentacion,
    { cantidad: String(s.cantidad), productos: Object.fromEntries(s.componentes.map(c => [c.idProducto, String(c.cantidad)])) }])))
  const [error, setError] = useState('')
  const [message, setMessage] = useState('')
  function change(id: number, quantity?: string, component?: number, value = '') {
    setValues(previous => {
      const prior = previous[id] ?? { cantidad: '', productos: {} }
      return { ...previous, [id]: { cantidad: quantity ?? prior.cantidad, productos: component === undefined ? prior.productos : { ...prior.productos, [component]: value } } }
    }); setError(''); setMessage('')
  }
  return <li className="public-category-card catalog-surtido-card">
    <CatalogImage url={product.urlImagen} name={product.nombre} kind="productos" />
    <h2>{product.nombre}</h2><span className="catalog-surtido-label">Surtido</span>
    {product.presentaciones.map(p => {
      const current = values[p.idProductoPresentacion] ?? { cantidad: '', productos: {} }
      const total = Object.values(current.productos).reduce((sum, v) => sum + (Number(v) || 0), 0)
      const required = (Number(current.cantidad) || 0) * p.unidadesEquivalentes
      const locked = lockedIds.includes(p.idProductoPresentacion)
      return <fieldset key={p.idProductoPresentacion} disabled={disabled || locked || !p.disponible} className="catalog-surtido-presentation" onKeyDown={event => {
        if (!(event.target instanceof HTMLInputElement) || event.nativeEvent.isComposing || event.ctrlKey || event.altKey || event.metaKey || !['Enter', 'ArrowDown', 'ArrowUp'].includes(event.key)) return
        event.preventDefault()
        const fields = Array.from(event.currentTarget.querySelectorAll<HTMLInputElement>('input:not(:disabled)'))
        const i = fields.indexOf(event.target); const delta = event.key === 'ArrowUp' || (event.key === 'Enter' && event.shiftKey) ? -1 : 1
        const next = fields[Math.max(0, Math.min(fields.length - 1, i + delta))]; next?.focus(); next?.select()
      }}>
        <legend>{p.nombre} · {money.format(p.precio)}</legend>
        <label>Cantidad<input type="number" inputMode="numeric" min="0" max="999" step="1" value={current.cantidad}
          onChange={e => change(p.idProductoPresentacion, e.target.value)} /></label>
        {product.productos.map(c => <label className="catalog-surtido-component" key={c.idProducto}>
          <span>{c.nombre}{!c.disponible && <small>No disponible</small>}</span>
          <input type="number" inputMode="numeric" min="0" max="2147483647" step="1" aria-label={`${product.nombre}, ${p.nombre}: unidades de ${c.nombre}`}
            value={current.productos[c.idProducto] ?? ''} disabled={!c.disponible} onChange={e => change(p.idProductoPresentacion, undefined, c.idProducto, e.target.value)} />
        </label>)}
        <small aria-live="polite">{total} de {required} unidades</small>
        {locked && <small>Ya está en la venta. Editala desde Nueva venta.</small>}
        {!p.disponible && <small>No disponible</small>}
      </fieldset>
    })}
    {error && <p role="alert" className="sale-catalog-error">{error}</p>}
    {message && <p role="status">{message}</p>}
    <button type="button" className="sale-catalog-add public-checkout-confirm" disabled={disabled || product.presentaciones.every(p => !p.disponible || lockedIds.includes(p.idProductoPresentacion))}
      onClick={() => {
        try {
          const selections = surtidoCatalogSelections(product, Object.fromEntries(Object.entries(values).filter(([id]) => !lockedIds.includes(Number(id)))))
          const failure = onAdd(selections)
          if (failure) setError(failure)
          else { setError(''); setMessage('Surtido agregado correctamente.') }
        } catch (failure) { setError(failure instanceof Error ? failure.message : 'Revisa las cantidades del surtido.') }
      }}>{actionLabel}</button>
  </li>
}
