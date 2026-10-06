import { changeBrandState } from '../api/brandsApi'
import { ApiError } from '../../../shared/api/ApiError'
import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { useBrands } from '../hooks/useBrands'
import { BrandList } from '../components/BrandList'
import type { BrandResponse } from '../types/brand'
import { BrandForm } from '../components/BrandForm'
import '../../../shared/ui/catalog.css'
import './BrandsPage.css'

export function BrandsPage() {
  const { session, logout } = useAuth()
  const { items, loading, error, reload } = useBrands(session?.user.role === 'ADMINISTRADOR')
  const [editing, setEditing] = useState<BrandResponse>()
  const [open, setOpen] = useState(false)
  const [notice, setNotice] = useState('')
  const [pending, setPending] = useState(false)
  const [stateError, setStateError] = useState('')
  const stateRequest = useRef<AbortController | null>(null)
  useEffect(() => () => stateRequest.current?.abort(), [])
  const panel = useRef<HTMLElement>(null)
  const add = useRef<HTMLButtonElement>(null)
  useEffect(() => { if (open) panel.current?.focus() }, [open])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  async function changeState(item: BrandResponse) {
    if (!session || session.user.role !== 'ADMINISTRADOR' || stateRequest.current) return
    const controller = new AbortController()
    stateRequest.current = controller
    setPending(true); setStateError(''); setNotice('')
    try {
      await changeBrandState(item.idMarca, !item.estado, session.token, controller.signal)
      if (!controller.signal.aborted) { setNotice(item.estado ? 'Marca desactivada.' : 'Marca activada.'); reload() }
    } catch (failure) {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setStateError(failure instanceof ApiError ? failure.message : 'No se pudo cambiar el estado.')
    } finally {
      stateRequest.current = null
      if (!controller.signal.aborted) setPending(false)
    }
  }
  function close() { setOpen(false); setEditing(undefined); requestAnimationFrame(() => add.current?.focus()) }
  return <div className="catalog-page">
    <p className="catalog-notice" role="status">{notice}</p>
    {stateError && <p className="brand-error" role="alert">{stateError}</p>}
    {open ? <section ref={panel} tabIndex={-1} className="brand-card catalog-editor" aria-labelledby="brand-title">
      <button type="button" className="catalog-close" aria-label="Cerrar formulario" title="Cerrar formulario" onClick={close}>×</button>
      <h2 id="brand-title">{editing ? "Editar marca" : "Crear marca"}</h2>
      <BrandForm key={editing?.idMarca ?? "new"} brand={editing} onSaved={() => { setNotice(editing ? 'Marca actualizada correctamente.' : 'Marca creada correctamente.'); reload(); close() }} />
    </section> : <>
      {/* El listado se oculta al abrir el formulario y se recarga después de guardar. */}
      <BrandList items={items} loading={loading} error={error} reload={reload} pending={pending} onStateChange={changeState} onEdit={item => { setEditing(item); setNotice(""); setOpen(true) }} />
      <button type="button" ref={add} disabled={pending || loading} className="catalog-add-button" aria-label="Crear marca" title="Crear marca"
        onClick={() => { setNotice(''); setOpen(true) }}><span aria-hidden="true">+</span></button>
    </>}
  </div>
}



