import { useEffect, useId, useRef, useState } from 'react'
import { ApiError } from '../../../shared/api/ApiError'
import { loadSaleCustomer } from '../api/customersApi'
import type { SaleCustomer, SaleCustomerDetail } from '../api/customersApi'

type Props = {
  customers: SaleCustomer[]; token: string; disabled: boolean; value: string; formId?: string
  onSelect: (customer: SaleCustomerDetail | null) => void
  onNext: () => void
  onNameChange: (value: string) => void; onPending: (pending: boolean) => void; onUnauthorized: () => void; onFailure: () => void
}

export function SaleCustomerPicker({ customers, token, disabled, value, formId, onSelect, onNext, onNameChange, onPending, onUnauthorized, onFailure }: Props) {
  const [open, setOpen] = useState(false)
  const [active, setActive] = useState(0)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const request = useRef<AbortController | null>(null)
  const list = useRef<HTMLUListElement>(null)
  const listId = useId()
  useEffect(() => () => request.current?.abort(), [token])
  const normalized = value.trim().toLocaleLowerCase()
  const matches = normalized ? customers.filter(c => c.nombre.toLocaleLowerCase().includes(normalized)) : []
  const expanded = open && matches.length > 0
  const index = Math.min(active, matches.length - 1)
  useEffect(() => { if (expanded) list.current?.children[index]?.scrollIntoView({ block: 'nearest' }) }, [expanded, index])

  function clearRequest() {
    request.current?.abort(); request.current = null
    setPending(false); onPending(false); setError('')
  }
  async function choose(customer: SaleCustomer) {
    if (disabled) return
    clearRequest(); onSelect(null); onNameChange(customer.nombre); setOpen(false)
    const controller = new AbortController()
    request.current = controller; setPending(true); onPending(true)
    try {
      const detail = await loadSaleCustomer(customer.id_cliente, token, controller.signal)
      if (!controller.signal.aborted && request.current === controller) onSelect(detail)
    } catch (failure) {
      if (controller.signal.aborted || request.current !== controller) return
      onFailure()
      if (failure instanceof ApiError && failure.status === 401) onUnauthorized()
      else setError(failure instanceof ApiError && failure.status === 404 ? 'El cliente ya no existe. Selecciona otro cliente.'
        : failure instanceof Error ? failure.message : 'No se pudieron cargar los datos del cliente.')
    } finally {
      if (request.current === controller) { request.current = null; setPending(false); onPending(false) }
    }
  }
  return <div className="sale-customer-picker" onBlur={event => {
    if (!event.currentTarget.contains(event.relatedTarget)) setOpen(false)
  }}>
    <div className="sale-customer-search">
      <label>Nombre<input form={formId} type="text" value={value} disabled={disabled} autoComplete="off" required maxLength={100}
        placeholder="Nombre del cliente" role="combobox" aria-autocomplete="list"
        aria-expanded={expanded} aria-controls={expanded ? listId : undefined}
        aria-activedescendant={expanded ? `${listId}-${matches[index].id_cliente}` : undefined}
        onFocus={() => setOpen(true)}
        onChange={event => { clearRequest(); onNameChange(event.target.value); setActive(0); setOpen(true) }}
        onKeyDown={event => {
          if (event.nativeEvent.isComposing) return
          if (event.key === 'Escape') { event.preventDefault(); setOpen(false); return }
          if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
            event.preventDefault(); setOpen(true)
            if (matches.length) setActive(value => !open ? (event.key === 'ArrowDown' ? 0 : matches.length - 1) : (Math.min(value, matches.length - 1) + (event.key === 'ArrowDown' ? 1 : -1) + matches.length) % matches.length)
          }
          if (event.key === 'Enter') {
            event.preventDefault()
            if (expanded) void choose(matches[index])
            else if (!pending && !disabled) onNext()
          }
        }} /></label>
    </div>
    {expanded && <ul ref={list} id={listId} role="listbox" className="sale-customer-options" aria-label="Clientes encontrados">
      {matches.map((customer, i) => <li key={customer.id_cliente} id={`${listId}-${customer.id_cliente}`} role="option"
        aria-selected={i === index} onMouseEnter={() => setActive(i)} onMouseDown={event => event.preventDefault()}
        onClick={() => void choose(customer)}>{customer.nombre}<small>#{customer.id_cliente}</small></li>)}
    </ul>}
    {pending && <p role="status">Cargando datos del cliente…</p>}
    {error && <p className="sales-error" role="alert">{error}</p>}
  </div>
}
