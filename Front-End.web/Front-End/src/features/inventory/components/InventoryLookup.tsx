import { useState } from 'react'

export interface InventoryOption { id: number; nombre: string }
export function InventoryLookup({ label, items, selected, loading, error, onSelect, query, onQueryChange }: {
  label: string; items: InventoryOption[]; selected?: number; loading: boolean; error: string
  query: string; onQueryChange: (value: string) => void
  onSelect: (id?: number) => void
}) {

  const [open, setOpen] = useState(false)
  const normalize = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es').trim()
  const matches = items.filter(item => normalize(item.nombre).includes(normalize(query)))
  const id = 'inventory-' + label.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '')
  return <div className="inventory-search inventory-lookup" onBlur={event => {
    if (!event.currentTarget.contains(event.relatedTarget)) setOpen(false)
  }}>
    <label htmlFor={id}>{label}</label>
    <input id={id} type="search" autoComplete="off" maxLength={100}
      value={query} placeholder={'Buscar ' + label.toLowerCase() + '…'}
      aria-expanded={open} aria-controls={id + '-options'}
      onFocus={() => setOpen(true)} onKeyDown={event => {
        if (event.key === 'Escape') setOpen(false)
        if (event.key === 'Enter' && matches.length === 1) {
          event.preventDefault(); onQueryChange(matches[0].nombre); onSelect(matches[0].id); setOpen(false)
        }
      }} onChange={event => {
        onQueryChange(event.target.value); setOpen(true)
        if (selected !== undefined) onSelect(undefined)
      }} />
    {open && <div id={id + '-options'} className="inventory-suggestions">
      {loading ? <p role="status">Cargando…</p> : error ? <p role="alert">{error}</p>
        : matches.length ? <ul aria-label={label + ': coincidencias'}>{matches.map(item => <li key={item.id}>
          <button type="button" onClick={() => { onQueryChange(item.nombre); onSelect(item.id); setOpen(false) }}>{item.nombre}</button>
        </li>)}</ul> : <p role="status">Sin coincidencias.</p>}
    </div>}
  </div>
}
