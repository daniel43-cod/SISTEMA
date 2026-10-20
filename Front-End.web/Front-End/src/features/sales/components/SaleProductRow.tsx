import { useId, useLayoutEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { ApiError } from '../../../shared/api/ApiError'
import { findBarcode } from '../api/salesApi'
import { filterSaleOptions, saleOptions } from '../utils/saleSearch'
import type { SaleDraft, SaleOption } from '../utils/saleSearch'
import { cashCents, saleLineTotal } from '../utils/saleValidation'
import type { SaleProduct } from '../utils/saleValidation'

const money = (amount: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(amount)
type Props = {
  row: SaleDraft; options: SaleOption[]; usedIds: number[]; token: string; disabled: boolean; autoFocus: boolean
  onChange: (row: SaleDraft) => void; onChoose: (option: SaleOption) => boolean
  onNext: () => void; onRemove: () => void; onPending: (pending: boolean) => void
}

export function SaleProductRow({ row, options, usedIds, token, disabled, autoFocus, onChange, onChoose, onNext, onRemove, onPending }: Props) {
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)
  const [barcodeProduct, setBarcodeProduct] = useState<SaleProduct>()
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const input = useRef<HTMLInputElement>(null)
  const barcodeInput = useRef<HTMLInputElement>(null)
  const quantity = useRef<HTMLInputElement>(null)
  const discount = useRef<HTMLInputElement>(null)
  const list = useRef<HTMLUListElement>(null)
  const touchStart = useRef<{ x: number; y: number } | null>(null)
  const selectedByTouch = useRef(false)
  const request = useRef<AbortController | null>(null)
  const listId = useId()
  const matches = barcodeProduct
    ? filterSaleOptions(saleOptions([barcodeProduct]), '', usedIds, barcodeProduct.id_producto)
    : filterSaleOptions(options, row.text, usedIds)
  const expanded = open && row.selection === null && matches.length > 0 && !disabled
  const index = Math.min(active, matches.length - 1)
  const discountCents = cashCents(row.selection?.discount ?? '0.00')
  const gross = row.selection ? Math.round(row.selection.presentation.precio * 100) * (Number(row.selection.quantity) || 0) : 0
  const discountError = row.selection && (discountCents === null || discountCents >= gross)
    ? 'Usa hasta dos decimales y un descuento menor al importe de la línea.' : ''
  let lineTotal: number | null = null
  if (row.selection) { try { lineTotal = saleLineTotal(row.selection) } catch { /* Se muestran los errores en los campos y al enviar. */ } }
  useLayoutEffect(() => () => request.current?.abort(), [])
  useLayoutEffect(() => {
    if (!expanded) return
    function position() {
      if (!input.current || !list.current) return
      const rect = (barcodeProduct ? barcodeInput.current : input.current)?.getBoundingClientRect()
      if (!rect) return
      const viewport = window.visualViewport
      const viewportTop = viewport?.offsetTop ?? 0
      const viewportBottom = viewportTop + (viewport?.height ?? window.innerHeight)
      const below = viewportBottom - rect.bottom - 12
      const above = rect.top - viewportTop - 12
      const upwards = below < 260 && above > below
      const width = Math.min(Math.max(rect.width, 320), window.innerWidth - 16)
      Object.assign(list.current.style, {
        left: `${Math.max(8, Math.min(rect.left, window.innerWidth - width - 8))}px`, width: `${width}px`,
        maxHeight: `${Math.max(44, Math.min(300, upwards ? above : below))}px`,
        top: upwards ? 'auto' : `${rect.bottom + 6}px`, bottom: upwards ? `${window.innerHeight - rect.top + 6}px` : 'auto',
      })
    }
    position()
    list.current?.children[index]?.scrollIntoView({ block: 'nearest' })
    window.addEventListener('scroll', position, true); window.addEventListener('resize', position)
    window.visualViewport?.addEventListener('resize', position)
    window.visualViewport?.addEventListener('scroll', position)
    return () => {
      window.removeEventListener('scroll', position, true); window.removeEventListener('resize', position)
      window.visualViewport?.removeEventListener('resize', position)
      window.visualViewport?.removeEventListener('scroll', position)
    }
  }, [expanded, index, row.text, matches.length, barcodeProduct])

  function choose(option: SaleOption) {
    if (disabled || !onChoose(option)) return
    setOpen(false); setError(''); setBarcodeProduct(undefined)
    requestAnimationFrame(() => quantity.current?.focus())
  }
  async function barcode() {
    if (disabled || !row.barcode?.trim() || pending) return
    request.current?.abort()
    const controller = new AbortController(); request.current = controller
    setPending(true); onPending(true); setError('')
    try {
      const product = await findBarcode(row.barcode, token, controller.signal)
      if (controller.signal.aborted || request.current !== controller) return
      if (!product.presentaciones.some(p => !usedIds.includes(p.id_producto_presentacion)))
        throw new Error('No hay presentaciones disponibles de ese producto para agregar en esta fila.')
      setBarcodeProduct(product); setActive(0); setOpen(true)
    } catch (failure) {
      if (!controller.signal.aborted) setError(failure instanceof ApiError && failure.status === 400 ? 'Producto no disponible.'
        : failure instanceof ApiError && failure.status === 404 ? 'Código no encontrado. Busca por nombre o revisa el código.'
        : failure instanceof Error ? failure.message : 'No se pudo buscar el producto.')
    } finally {
      if (request.current === controller) { request.current = null; setPending(false); onPending(false) }
    }
  }
  return <tr>
    <td className="sale-barcode-cell"><input ref={barcodeInput} type="text" aria-label="Código de barras" autoComplete="off"
      value={row.barcode ?? ''} disabled={disabled} placeholder="Escanea o escribe el código"
      aria-describedby={error ? `${listId}-error` : undefined}
      onBlur={() => { setOpen(false); request.current?.abort(); request.current = null; setPending(false); onPending(false) }}
      onChange={event => {
        request.current?.abort(); request.current = null; setPending(false); onPending(false)
        setBarcodeProduct(undefined); setError(''); setOpen(false)
        onChange({ ...row, text: '', barcode: event.target.value, selection: null })
      }}
      onKeyDown={event => {
        if (event.key === 'Enter' && !event.nativeEvent.isComposing) {
          event.preventDefault()
          if (expanded) choose(matches[index])
          else void barcode()
        }
        if (event.key === 'Escape') { event.preventDefault(); setOpen(false) }
        if ((event.key === 'ArrowDown' || event.key === 'ArrowUp') && expanded) {
          event.preventDefault()
          setActive(value => (Math.min(value, matches.length - 1) + (event.key === 'ArrowDown' ? 1 : -1) + matches.length) % matches.length)
        }
      }} /></td>
    <td className="sale-product-cell">
      <input ref={input} data-sale-row={row.key} aria-label="Producto y presentación" type="text" autoComplete="off" autoFocus={autoFocus}
        value={row.text} disabled={disabled} placeholder="Busca por nombre…" role="combobox" aria-autocomplete="list"
        aria-expanded={expanded} aria-controls={expanded ? listId : undefined}
        aria-activedescendant={expanded ? `${listId}-${matches[index].presentation.id_producto_presentacion}` : undefined}
        aria-describedby={error ? `${listId}-error` : undefined}
        onFocus={() => { if (!row.selection) setOpen(true) }} onBlur={() => {
          setOpen(false); request.current?.abort(); request.current = null; setPending(false); onPending(false)
        }}
        onChange={event => {
          request.current?.abort(); request.current = null; setPending(false); onPending(false)
          setBarcodeProduct(undefined); setError(''); setOpen(true); setActive(0)
          onChange({ ...row, text: event.target.value, barcode: '', selection: null })
        }}
        onKeyDown={event => {
          if (event.nativeEvent.isComposing) return
          if (event.key === 'Escape') { event.preventDefault(); setOpen(false) }
          if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault(); setOpen(true)
            if (matches.length) setActive(value => !open ? (event.key === 'ArrowDown' ? 0 : matches.length - 1)
              : (Math.min(value, matches.length - 1) + (event.key === 'ArrowDown' ? 1 : -1) + matches.length) % matches.length)
          }
          if (event.key === 'Enter') {
            event.preventDefault()
            if (expanded) choose(matches[index])
            else if (row.selection) quantity.current?.focus()
          }
        }} />
      {pending && <small role="status">Buscando código…</small>}
      {error && <small id={`${listId}-error`} className="sales-error" role="alert">{error}</small>}
      {open && row.text.trim() && !row.selection && !matches.length && !pending && !error && <small role="status">Sin coincidencias por nombre.</small>}
      {expanded && createPortal(<ul ref={list} id={listId} role="listbox" className="sale-product-options" aria-label="Productos y presentaciones">
        {matches.map((option, i) => <li key={option.presentation.id_producto_presentacion} id={`${listId}-${option.presentation.id_producto_presentacion}`}
          role="option" aria-selected={i === index} onPointerMove={event => { if (event.pointerType === 'mouse') setActive(i) }}
          onPointerDown={event => {
            event.preventDefault()
            selectedByTouch.current = false
            touchStart.current = { x: event.clientX, y: event.clientY }
          }}
          onPointerCancel={() => { touchStart.current = null }}
          onPointerUp={event => {
            const start = touchStart.current
            touchStart.current = null
            if (event.pointerType !== 'mouse' && start && Math.hypot(event.clientX - start.x, event.clientY - start.y) < 10) {
              selectedByTouch.current = true
              choose(option)
            }
          }}
          onClick={() => { if (!selectedByTouch.current) choose(option) }}>
          <strong>{option.label}</strong><span>{money(option.presentation.precio)} · {Math.floor(Math.max(0, option.product.stock) / option.presentation.unidades_equivalentes)} disponibles</span>
        </li>)}
      </ul>, document.body)}
    </td>
    <td><input ref={quantity} type="number" inputMode="numeric" min="1" step="1" disabled={disabled || !row.selection} required={!!row.selection}
      aria-label={`Cantidad de ${row.text || 'producto'}`} value={row.selection?.quantity ?? ''}
      onChange={event => { if (row.selection) onChange({ ...row, selection: { ...row.selection, quantity: event.target.value } }) }}
      onKeyDown={event => {
        if (event.key === 'Enter' && !event.nativeEvent.isComposing) {
          event.preventDefault(); if (quantity.current?.reportValidity() && row.selection) { discount.current?.focus(); discount.current?.select() }
        }
      }} /></td>
    <td>{row.selection ? money(row.selection.presentation.precio) : '—'}</td>
    <td className="sale-discount-cell"><input ref={discount} type="text" inputMode="decimal"
      pattern="[0-9]{1,8}([.,][0-9]{1,2})?" required={!!row.selection} disabled={disabled || !row.selection}
      aria-label={`Descuento en quetzales de ${row.text || 'producto'}`} aria-invalid={!!discountError}
      aria-describedby={discountError ? `${listId}-discount-error` : undefined}
      value={row.selection?.discount ?? (row.selection ? '0.00' : '')} placeholder="0.00"
      onChange={event => { if (row.selection) onChange({ ...row, selection: { ...row.selection, discount: event.target.value } }) }}
      onKeyDown={event => {
        if (event.key === 'Enter' && !event.nativeEvent.isComposing) {
          event.preventDefault()
          if (discount.current?.reportValidity() && !discountError && lineTotal !== null) onNext()
        }
      }} />{discountError && <small id={`${listId}-discount-error`} className="sale-discount-error">{discountError}</small>}</td>
    <td>{lineTotal !== null ? money(lineTotal / 100) : '—'}</td>
    <td><button type="button" className="sale-remove-line" disabled={disabled} onClick={onRemove} aria-label={`Quitar ${row.text || 'fila'}`}>
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M3 6h18M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2M5 6l1 14a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1l1-14M10 10v7M14 10v7" />
      </svg>
    </button></td>
  </tr>
}
