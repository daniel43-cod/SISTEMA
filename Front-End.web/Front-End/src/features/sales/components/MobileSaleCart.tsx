import { useEffect, useId, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { findBarcode } from '../api/salesApi'
import { filterSaleOptions, saleOptions } from '../utils/saleSearch'
import type { SaleDraft, SaleOption } from '../utils/saleSearch'
import { saleLineTotal } from '../utils/saleValidation'

const money = (amount: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(amount)

type Props = {
  rows: SaleDraft[]; options: SaleOption[]; token: string; disabled: boolean
  onChoose: (option: SaleOption, quantity: string, discount: string) => boolean; onQuantity: (key: number, quantity: number) => void
  onRemove: (key: number) => void; onDiscount: (key: number, value: string) => void
}

export function MobileSaleCart({ rows, options, token, disabled, onChoose, onQuantity, onRemove, onDiscount }: Props) {
  const [query, setQuery] = useState('')
  const [editor, setEditor] = useState<{ option: SaleOption; quantity: string; discount: string } | null>(null)
  const quantityInput = useRef<HTMLInputElement>(null)
  const searchInput = useRef<HTMLInputElement>(null)
  const editorDialog = useRef<HTMLDialogElement>(null)
  const editingPresentationId = editor?.option.presentation.id_producto_presentacion
  useEffect(() => {
    if (editingPresentationId === undefined) return
    const dialog = editorDialog.current
    const viewport = window.visualViewport
    function positionEditor() {
      const height = viewport?.height ?? window.innerHeight
      const visibleTop = viewport?.offsetTop ?? 0
      dialog?.style.setProperty('--editor-visible-height', `${Math.max(1, height - 24)}px`)
      const dialogHeight = dialog?.getBoundingClientRect().height ?? 0
      const top = visibleTop + Math.max(12, height - dialogHeight - 16)
      dialog?.style.setProperty('--editor-visible-top', `${top}px`)
    }
    positionEditor()
    dialog?.showModal()
    positionEditor()
    const dialogObserver = new ResizeObserver(positionEditor)
    if (dialog) dialogObserver.observe(dialog)
    quantityInput.current?.focus({ preventScroll: true })
    viewport?.addEventListener('resize', positionEditor)
    viewport?.addEventListener('scroll', positionEditor)
    window.addEventListener('resize', positionEditor)
    return () => {
      viewport?.removeEventListener('resize', positionEditor)
      viewport?.removeEventListener('scroll', positionEditor)
      window.removeEventListener('resize', positionEditor)
      dialogObserver.disconnect()
      dialog?.close()
    }
  }, [editingPresentationId])
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)
  const [barcodeOptions, setBarcodeOptions] = useState<SaleOption[] | null>(null)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const request = useRef<AbortController | null>(null)
  const id = useId()
  const matches = barcodeOptions ?? filterSaleOptions(options, query, [])
  const visible = open && matches.length > 0
  const index = Math.max(0, Math.min(active, matches.length - 1))
  const selectedRows = rows.filter(row => row.selection !== null)
  useEffect(() => () => request.current?.abort(), [])

  function choose(option: SaleOption) {
    if (disabled) return
    const existing = rows.find(row => row.selection?.presentation.id_producto_presentacion === option.presentation.id_producto_presentacion)
    setEditor({ option, quantity: existing?.selection?.quantity ?? '', discount: existing?.selection?.discount ?? '0.00' })
    request.current?.abort()
    setQuery(''); setOpen(false); setBarcodeOptions(null); setError(''); setPending(false)
  }

  async function searchCode() {
    if (!query.trim() || disabled || pending) return
    request.current?.abort()
    const controller = new AbortController()
    request.current = controller
    setPending(true); setError('')
    try {
      const product = await findBarcode(query, token, controller.signal)
      if (controller.signal.aborted) return
      const found = saleOptions([product])
      if (found.length === 1) choose(found[0]!)
      else { setBarcodeOptions(found); setOpen(true); setActive(0) }
    } catch (failure) {
      if (!controller.signal.aborted) setError(failure instanceof Error ? failure.message : 'No se pudo buscar el código.')
    } finally {
      if (request.current === controller) { setPending(false); request.current = null }
    }
  }

  return <div className="sales-mobile-cart">
    <div className="sales-cart-search">
      <label className="sr-only" htmlFor={id}>Buscar producto o código</label>
      <input ref={searchInput} data-mobile-cart-search id={id} type="search" placeholder="Buscar producto o código" autoComplete="off"
        value={query} disabled={disabled} role="combobox" aria-autocomplete="list" aria-expanded={visible}
        aria-controls={visible ? `${id}-matches` : undefined}
        aria-activedescendant={visible ? `${id}-option-${index}` : undefined}
        onChange={event => {
          request.current?.abort(); request.current = null; setPending(false)
          setQuery(event.target.value); setBarcodeOptions(null); setActive(0); setOpen(true); setError('')
        }}
        onFocus={() => setOpen(true)} onBlur={() => setOpen(false)}
        onKeyDown={event => {
          if (event.nativeEvent.isComposing) return
          if (event.key === 'Escape') { event.preventDefault(); setOpen(false) }
          if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault(); setOpen(true)
            if (matches.length) setActive((index + (event.key === 'ArrowDown' ? 1 : -1) + matches.length) % matches.length)
          }
          if (event.key === 'Enter') {
            event.preventDefault()
            if (visible) choose(matches[index]!)
            else void searchCode()
          }
        }} />
      {visible && <ul id={`${id}-matches`} className="sales-cart-matches" role="listbox" aria-label="Productos y presentaciones">
        {matches.map((option, i) => <li key={option.presentation.id_producto_presentacion} id={`${id}-option-${i}`}
          role="option" aria-selected={i === index} onPointerDown={event => event.preventDefault()} onClick={() => choose(option)}>
          <strong>{option.label}</strong><span>{money(option.presentation.precio)}</span>
        </li>)}
      </ul>}
    </div>
    {pending && <p role="status">Buscando código…</p>}
    {error && <p role="alert" className="sales-error">{error}</p>}
    {query.trim() && !matches.length && !pending && <button type="button" disabled={disabled} onClick={() => void searchCode()}>Buscar código</button>}
    {editor && createPortal(<dialog ref={editorDialog} className="sales-page sales-cart-editor-dialog" aria-labelledby={`${id}-editor-title`}
      onCancel={event => { event.preventDefault(); setEditor(null); setError(''); searchInput.current?.focus() }}>
      <fieldset className="sales-cart-product-editor" disabled={disabled} aria-labelledby={`${id}-editor-title`}>
      <legend id={`${id}-editor-title`}>{editor.option.product.nombre}</legend>
      <p>{editor.option.presentation.presentacion} · {money(editor.option.presentation.precio)} c/u</p>
      <div className="sales-cart-editor-fields">
        <label>Cantidad
          <input ref={quantityInput} type="number" inputMode="numeric" min="1" step="1" placeholder="Cantidad"
            value={editor.quantity} onChange={event => { setEditor({ ...editor, quantity: event.target.value }); setError('') }}
            onKeyDown={event => {
              if (event.key === 'Enter') {
                event.preventDefault()
                event.currentTarget.closest('fieldset')?.querySelector<HTMLInputElement>('[data-editor-discount]')?.focus()
              }
            }} />
        </label>
        <label>Descuento (Q)
          <input data-editor-discount type="text" inputMode="decimal" placeholder="0.00" value={editor.discount}
            onChange={event => { setEditor({ ...editor, discount: event.target.value }); setError('') }}
            onKeyDown={event => { if (event.key === 'Enter') event.preventDefault() }} />
        </label>
      </div>
      {error && <p role="alert" className="sales-error">{error}</p>}
      <div className="sales-cart-editor-actions">
        <button type="button" onClick={() => { setEditor(null); setError(''); searchInput.current?.focus() }}>Cancelar</button>
        <button type="button" onClick={() => {
          try {
            saleLineTotal({ product: editor.option.product, presentation: editor.option.presentation, quantity: editor.quantity, discount: editor.discount })
            if (!onChoose(editor.option, editor.quantity, editor.discount)) { setError('No se pudo agregar el producto. Revisa las existencias y los importes.'); return }
            setEditor(null); setError(''); searchInput.current?.focus()
          } catch (failure) { setError((failure as Error).message); quantityInput.current?.focus() }
        }}>Listo</button>
      </div>
    </fieldset></dialog>, document.body)}
    <h3>Carrito ({selectedRows.length})</h3>
    {!selectedRows.length && <p>Busca un producto para agregarlo.</p>}
    <ul className="sales-cart-items">
      {selectedRows.map(row => {
        const line = row.selection!
        let total: number | null = null
        let lineError = ''
        try { total = saleLineTotal(line) / 100 } catch (failure) { lineError = (failure as Error).message }
        const usedUnits = selectedRows.reduce((sum, item) => item.selection!.product.id_producto === line.product.id_producto
          ? sum + (Number(item.selection!.quantity) || 0) * item.selection!.presentation.unidades_equivalentes : sum, 0)
        const remaining = Math.max(0, line.product.stock - usedUnits)
        return <li key={row.key} className="sales-cart-item">
          <div className="sales-cart-item-heading"><strong>{line.product.nombre}</strong><strong>{total === null ? 'Revisar' : money(total)}</strong></div>
          <small>{line.presentation.presentacion} · {money(line.presentation.precio)} c/u</small>
          <div className="sales-cart-item-actions">
            <div className="sales-cart-quantity">
              <button type="button" disabled={disabled || Number(line.quantity) <= 1} aria-label={`Reducir cantidad de ${line.product.nombre}`} onClick={() => onQuantity(row.key, Number(line.quantity) - 1)}>−</button>
              <span aria-label="Cantidad">{line.quantity}</span>
              <button type="button" disabled={disabled} aria-label={`Aumentar cantidad de ${line.product.nombre}`} onClick={() => onQuantity(row.key, Number(line.quantity) + 1)}>+</button>
            </div>
            <label className="sales-cart-discount">Descuento (Q)
              <input type="text" inputMode="decimal" value={line.discount ?? '0.00'} placeholder="0.00"
                disabled={disabled} aria-label={`Descuento en quetzales de ${line.product.nombre}`}
                aria-invalid={!!lineError} aria-describedby={lineError ? `${id}-discount-${row.key}` : undefined}
                onChange={event => onDiscount(row.key, event.target.value)}
                onKeyDown={event => { if (event.key === 'Enter') event.preventDefault() }} />
            </label>
            <button type="button" className="sales-cart-remove" disabled={disabled} aria-label={`Quitar ${line.product.nombre}`} onClick={() => onRemove(row.key)}>
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M3 6h18M9 6V4h6v2M5 6l1 14h12l1-14M10 10v6M14 10v6" /></svg>
            </button>
          </div>
          {lineError && <small id={`${id}-discount-${row.key}`} className="sale-discount-error" role="alert">{lineError}</small>}
          {remaining <= line.presentation.unidades_equivalentes * 3 && <small className="sales-cart-stock">Quedan {remaining} uds</small>}
        </li>
      })}
    </ul>
  </div>
}