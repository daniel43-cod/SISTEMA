import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import type { SaleLine } from '../utils/saleValidation'
import { saleTotals, surtidoLineFromInputs } from '../utils/saleValidation'
import type { SaleOption } from '../utils/saleSearch'
import './SaleSurtidoEditor.css'

export function SaleSurtidoEditor({ option, initial, existing, disabled, onSave, onClose }: {
  option: SaleOption; initial?: SaleLine; existing: SaleLine[]; disabled: boolean
  onSave: (line: SaleLine) => string | null; onClose: () => void
}) {
  const dialog = useRef<HTMLDialogElement>(null)
  const first = useRef<HTMLInputElement>(null)
  const [quantity, setQuantity] = useState(initial?.quantity ?? '1')
  const [discount, setDiscount] = useState(initial?.discount ?? '')
  const [values, setValues] = useState<Record<number, string>>(() => Object.fromEntries(initial?.componentes?.filter(c => option.product.componentes?.some(p => p.id_producto === c.id_producto)).map(c => [c.id_producto, String(c.cantidad)]) ?? []))
  const [error, setError] = useState('')
  const components = option.product.componentes ?? []
  const required = Number(quantity) * option.presentation.unidades_equivalentes
  const total = Object.values(values).reduce((sum, v) => sum + (Number(v) || 0), 0)
  useEffect(() => {
    const element = dialog.current
    const previous = document.activeElement instanceof HTMLElement ? document.activeElement : null
    const viewport = window.visualViewport
    const position = () => {
      element?.style.setProperty('--surtido-height', `${Math.max(1, (viewport?.height ?? window.innerHeight) - 24)}px`)
      element?.style.setProperty('--surtido-top', `${(viewport?.offsetTop ?? 0) + 12}px`)
    }
    position(); element?.showModal(); first.current?.focus({ preventScroll: true })
    viewport?.addEventListener('resize', position); viewport?.addEventListener('scroll', position)
    const overflow = document.body.style.overflow; document.body.style.overflow = 'hidden'
    return () => {
      element?.close(); document.body.style.overflow = overflow; previous?.focus()
      viewport?.removeEventListener('resize', position); viewport?.removeEventListener('scroll', position)
    }
  }, [])
  return createPortal(<dialog ref={dialog} className="sale-surtido-dialog" aria-labelledby="sale-surtido-title"
    onCancel={event => { event.preventDefault(); if (!disabled) onClose() }}>
    <form onSubmit={event => {
      event.preventDefault()
      if (disabled) return
      try {
        const line = surtidoLineFromInputs(option.product, option.presentation, quantity, discount, values)
        saleTotals([...existing, line])
        const message = onSave(line)
        if (message) setError(message)
      } catch (failure) { setError(failure instanceof Error ? failure.message : 'Revisa las cantidades.') }
    }}>
      <h2 id="sale-surtido-title">{option.product.nombre}</h2>
      <fieldset disabled={disabled} onKeyDown={event => {
        if (!(event.target instanceof HTMLInputElement) || event.nativeEvent.isComposing || event.altKey || event.ctrlKey || event.metaKey ||
            !['Enter', 'ArrowDown', 'ArrowUp'].includes(event.key)) return
        event.preventDefault()
        const fields = Array.from(event.currentTarget.querySelectorAll<HTMLInputElement>('input:not(:disabled)'))
        const index = fields.indexOf(event.target)
        if (index < 0) return
        const direction = event.key === 'ArrowUp' || (event.key === 'Enter' && event.shiftKey) ? -1 : 1
        const next = fields[Math.max(0, Math.min(fields.length - 1, index + direction))]
        next.focus({ preventScroll: true }); next.select(); next.scrollIntoView({ block: 'nearest' })
      }}>
        <label>Cantidad de presentaciones<input type="number" min="1" max="2147483647" step="1" inputMode="numeric" required value={quantity} onChange={e => { setQuantity(e.target.value); setError('') }} /></label>
        <p role="status" className={total === required ? 'surtido-complete' : ''}>{total} de {Number.isSafeInteger(required) ? required : '—'} unidades</p>
        {components.map((c, i) => {
          const used = existing.reduce((sum, line) => sum + (line.product.id_surtido
            ? line.componentes?.find(item => item.id_producto === c.id_producto)?.cantidad ?? 0
            : line.product.id_producto === c.id_producto ? Number(line.quantity) * line.presentation.unidades_equivalentes : 0), 0)
          return <label key={c.id_producto} className="sale-surtido-component"><span>{c.nombre}<small>Disponibles: {Math.max(0, c.stock - used)}</small></span>
            <input ref={i === 0 ? first : undefined} type="number" min="0" max="2147483647" step="1" inputMode="numeric" placeholder="0"
              aria-label={`Unidades de ${c.nombre}`} value={values[c.id_producto] ?? ''} onChange={e => { setValues(v => ({ ...v, [c.id_producto]: e.target.value })); setError('') }}
              /></label>
        })}
        <label>Descuento (Q)<input type="text" inputMode="decimal" placeholder="0.00" value={discount} onChange={e => setDiscount(e.target.value)} /></label>
      </fieldset>
      {(option.product.bloqueo || error) && <p role="alert" className="sales-error">{option.product.bloqueo || error}</p>}
      <div className="sale-surtido-actions"><button type="button" disabled={disabled} onClick={onClose}>Cancelar</button>
        <button type="submit" disabled={disabled || !!option.product.bloqueo || total !== required || !Number.isSafeInteger(required) || required < 1 || required > 2147483647}>Agregar a la venta</button></div>
    </form>
  </dialog>, document.body)
}
