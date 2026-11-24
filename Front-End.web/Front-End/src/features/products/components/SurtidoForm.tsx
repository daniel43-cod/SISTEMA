import { useId, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useAuth } from '../../auth'
import { useBrands } from '../../brands'
import { usePresentations } from '../../presentations'
import { ApiError } from '../../../shared/api/ApiError'
import { useProductNames } from '../hooks/useProductNames'
import { filterProductNames } from '../api/searchProducts'
import type { ProductSuggestion } from '../api/searchProducts'
import { createSurtido } from '../api/surtidosApi'
import './SurtidoForm.css'

type Row = { key: number; presentation: string; units: string; price: string }
export function SurtidoForm({ onSaved, onCancel, onPending }: {
  onSaved: () => void; onCancel: () => void; onPending: (pending: boolean) => void
}) {
  const { session, logout } = useAuth()
  const brands = useBrands()
  const presentations = usePresentations()
  const [revision, setRevision] = useState(0)
  const names = useProductNames(revision)
  const [name, setName] = useState('')
  const [brand, setBrand] = useState('')
  const [text, setText] = useState('')
  const [selected, setSelected] = useState<ProductSuggestion[]>([])
  const [rows, setRows] = useState<Row[]>([{ key: 0, presentation: '', units: '', price: '' }])
  const nextKey = useRef(1)
  const [pending, setPending] = useState(false)
  const submitting = useRef(false)
  const [error, setError] = useState('')
  const errorRef = useRef<HTMLParagraphElement>(null)
  const searchRef = useRef<HTMLInputElement>(null)
  const id = useId()
  const suggestions = filterProductNames(names.items.filter(p => !selected.some(s => s.idProducto === p.idProducto)), text)
  const catalogError = brands.error || presentations.error || names.error
  function update(key: number, field: 'presentation' | 'units' | 'price', value: string) {
    setRows(current => current.map(row => row.key === key ? { ...row, [field]: value } : row))
  }
  function fail(message: string) {
    setError(message)
    requestAnimationFrame(() => { errorRef.current?.focus(); errorRef.current?.scrollIntoView({ block: 'nearest' }) })
  }
  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (submitting.current || !session) return
    setError('')
    if (!name.trim()) { fail('Ingresa el nombre del surtido.'); return }
    if (!selected.length) { fail('Selecciona al menos un producto para el surtido.'); searchRef.current?.focus(); return }
    if (!brands.items.some(b => b.idMarca === Number(brand)) ||
        rows.some(r => !presentations.items.some(p => p.idPresentacion === Number(r.presentation)))) {
      fail('Selecciona una marca y presentaciones disponibles.'); return
    }
    if (rows.some(r => !/^\d+$/.test(r.units) || !Number.isSafeInteger(Number(r.units)) || Number(r.units) < 1 || Number(r.units) > 2147483647 ||
        !/^\d+(\.\d{1,2})?$/.test(r.price) || Number(r.price) <= 0 || Number(r.price) > 99999999.99)) {
      fail('Ingresa unidades enteras mayores a cero y precios positivos con hasta dos decimales.'); return
    }
    if (new Set(rows.map(r => r.presentation)).size !== rows.length) { fail('No repitas una presentación.'); return }
    submitting.current = true; setPending(true); onPending(true)
    try {
      // Los costos actuales y su compatibilidad se validan en el servidor al guardar.
      await createSurtido({ nombre: name.trim(), idMarca: Number(brand), idsProductos: selected.map(p => p.idProducto),
        presentaciones: rows.map(r => ({ idPresentacion: Number(r.presentation), unidadesEquivalentes: Number(r.units), precio: Number(r.price) })) }, session.token)
      onSaved()
    } catch (failure) {
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else fail(failure instanceof ApiError ? failure.message : 'No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
    } finally { submitting.current = false; setPending(false); onPending(false) }
  }
  return <form className="surtido-form" onSubmit={save} aria-busy={pending}>
    {catalogError && <div role="alert" className="product-error">{catalogError} <button type="button" onClick={() => {
      brands.reload(); presentations.reload(); setRevision(v => v + 1)
    }}>Reintentar</button></div>}
    <fieldset className="surtido-section surtido-identity" disabled={pending}>
      <legend>Datos del surtido</legend>
      <label className="field" htmlFor={`${id}-name`}>Nombre del surtido
        <input id={`${id}-name`} className="input" required maxLength={200} value={name} onChange={e => setName(e.target.value)} placeholder="Ej. Super cola 3.3 surtido" /></label>
      <label className="field" htmlFor={`${id}-brand`}>Marca
        <select id={`${id}-brand`} className="input" required value={brand} onChange={e => setBrand(e.target.value)} disabled={brands.loading || !!brands.error}>
          <option value="">Selecciona una marca</option>
          {brands.items.map(b => <option key={b.idMarca} value={b.idMarca}>{b.nombre}{b.nombreCategoria ? ` — ${b.nombreCategoria}` : ''}</option>)}
        </select></label>
    </fieldset>
    <fieldset className="surtido-section" disabled={pending}>
      <legend>Productos permitidos ({selected.length})</legend>
      <label className="field" htmlFor={`${id}-search`}>Buscar productos
        <input ref={searchRef} id={`${id}-search`} type="search" className="input" maxLength={200} value={text}
          placeholder="Escribe al menos 3 letras del nombre" onChange={e => setText(e.target.value)}
          onKeyDown={e => { if (e.key === 'Enter') e.preventDefault() }} /></label>
      {names.loading && <p role="status">Cargando productos…</p>}
      {!!suggestions.length && <ul className="surtido-suggestions" aria-label="Coincidencias de productos">
        {suggestions.map(p => <li key={p.idProducto}><button type="button" onClick={() => {
          setSelected(current => current.some(s => s.idProducto === p.idProducto) ? current : [...current, p]); setText(''); searchRef.current?.focus()
        }}><span>{p.nombre}</span><span aria-hidden="true">+</span><span className="sr-only">Seleccionar</span></button></li>)}
      </ul>}
      {text.trim().length >= 3 && !names.loading && !names.error && !suggestions.length && <p role="status">No hay más coincidencias disponibles.</p>}
      <ul className="surtido-selected" aria-label="Productos seleccionados">
        {selected.map(p => <li key={p.idProducto}><strong>{p.nombre}</strong><button type="button" aria-label={`Quitar ${p.nombre}`}
          onClick={() => setSelected(current => current.filter(s => s.idProducto !== p.idProducto))}>×</button></li>)}
      </ul>
      {!selected.length && <p>Selecciona los productos que se podrán combinar.</p>}
      <p className="surtido-help">Al guardar se comprobarán los costos promedio de los productos seleccionados.</p>
    </fieldset>
    <fieldset className="surtido-section" disabled={pending}>
      <legend>Presentaciones de venta</legend>
      {rows.map((r, i) => <div className="surtido-presentation" key={r.key}>
        <label className="field" htmlFor={`${id}-presentation-${r.key}`}>Presentación
          <select id={`${id}-presentation-${r.key}`} className="input" required value={r.presentation} onChange={e => update(r.key, 'presentation', e.target.value)} disabled={presentations.loading || !!presentations.error}>
            <option value="">Selecciona una presentación</option>
            {presentations.items.map(p => <option key={p.idPresentacion} value={p.idPresentacion}>{p.descripcion}</option>)}
          </select></label>
        <label className="field" htmlFor={`${id}-units-${r.key}`}>Unidades equivalentes
          <input id={`${id}-units-${r.key}`} className="input" type="number" inputMode="numeric" min="1" max="2147483647" step="1" required value={r.units} onChange={e => update(r.key, 'units', e.target.value)} /></label>
        <label className="field" htmlFor={`${id}-price-${r.key}`}>Precio de venta (Q)
          <input id={`${id}-price-${r.key}`} className="input" type="number" inputMode="decimal" min="0.01" max="99999999.99" step="0.01" required value={r.price} onChange={e => update(r.key, 'price', e.target.value)} /></label>
        <button type="button" className="surtido-remove" disabled={rows.length === 1} aria-label={`Quitar presentación ${i + 1}`}
          onClick={() => setRows(current => current.filter(row => row.key !== r.key))}>×</button>
      </div>)}
      <button type="button" className="surtido-secondary" onClick={() => setRows(current => [...current, { key: nextKey.current++, presentation: '', units: '', price: '' }])}>+ Agregar presentación</button>
      <p className="surtido-help">Cada precio debe cubrir todas las unidades usando el producto de mayor costo promedio.</p>
    </fieldset>
    <p ref={errorRef} tabIndex={-1} role="alert" className="product-error">{error}</p>
    <div className="surtido-actions"><button type="button" className="surtido-secondary" disabled={pending} onClick={onCancel}>Cancelar</button>
      <button className="button" type="submit" disabled={pending || brands.loading || presentations.loading || !!brands.error || !!presentations.error}>
        {pending ? 'Guardando…' : 'Guardar surtido'}</button></div>
  </form>
}
