import { useEffect, useRef, useState } from 'react'
import '../../../shared/ui/catalog.css'
import { useAuth } from '../../auth'
import { PresentationForm } from '../components/PresentationForm'
import { EditPresentationForm } from '../components/EditPresentationForm'
import { PresentationList } from '../components/PresentationList'
import { usePresentations } from '../hooks/usePresentations'
import type { PresentationResponse } from '../types/presentation'
import { changePresentationState } from '../api/presentationsApi'
import { ApiError } from '../../../shared/api/ApiError'
import './PresentationsPage.css'

export function PresentationsPage() {
  const { session, logout } = useAuth()
  const { items, loading, error, reload } = usePresentations(session?.user.role === 'ADMINISTRADOR')
  const [editing, setEditing] = useState<PresentationResponse | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const addButton = useRef<HTMLButtonElement>(null)
  const [pending, setPending] = useState(false)
  const [stateError, setStateError] = useState('')
  const stateRequest = useRef<AbortController | null>(null)
  useEffect(() => () => stateRequest.current?.abort(), [])
  const [notice, setNotice] = useState('')
  const editorRef = useRef<HTMLElement>(null)
  const editTrigger = useRef<HTMLElement | null>(null)
  useEffect(() => {
    if (formOpen) {
      const panel = document.getElementById('catalog-editor')
      panel?.focus()
      panel?.scrollIntoView({ block: 'start', behavior: 'smooth' })
    }
  }, [formOpen])
  if (!session) return null
  const canEdit = session.user.role === 'ADMINISTRADOR'
  async function changeState(item: PresentationResponse) {
    if (!session || !canEdit || stateRequest.current) return
    const controller = new AbortController()
    stateRequest.current = controller
    setPending(true); setStateError(''); setNotice('')
    try {
      await changePresentationState(item.idPresentacion, !item.estado, session.token, controller.signal)
      if (!controller.signal.aborted) { setNotice(item.estado ? 'Presentación desactivada.' : 'Presentación activada.'); reload() }
    } catch (failure) {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setStateError(failure instanceof ApiError ? failure.message : 'No se pudo cambiar el estado.')
    } finally {
      stateRequest.current = null
      if (!controller.signal.aborted) setPending(false)
    }
  }
  function startEdit(item: PresentationResponse) {
    editTrigger.current = document.activeElement instanceof HTMLElement ? document.activeElement : null
    setNotice('')
    setEditing(item)
    setFormOpen(true)
    editorRef.current?.focus()
    editorRef.current?.scrollIntoView({ block: 'nearest', behavior: 'instant' })
  }
  function closeForm() {
    setFormOpen(false); setEditing(null)
    requestAnimationFrame(() => addButton.current?.focus())
  }
  return <div className="presentations-page catalog-page">
    <p className="catalog-notice" role="status">{notice}</p>
    {stateError && <p className="presentation-error" role="alert">{stateError}</p>}
    {canEdit && formOpen && <section id="catalog-editor" className="presentation-card catalog-editor" aria-labelledby="presentation-form-title" ref={editorRef} tabIndex={-1}>
      <span className="presentation-label">Presentaciones</span>
      <h2 id="presentation-form-title">{editing ? 'Editar presentación' : 'Crear presentación'}</h2>
      
      <button type="button" className="catalog-close" aria-label="Cerrar formulario" title="Cerrar formulario" onClick={closeForm}><span aria-hidden="true">×</span></button>
      {editing ? <EditPresentationForm key={editing.idPresentacion} presentation={editing}
        onCancel={closeForm}
        onSaved={() => { setNotice('Presentación actualizada correctamente.'); reload(); closeForm() }} />
        : <PresentationForm onCreated={() => { reload(); setNotice('Presentación creada correctamente.'); closeForm() }} />}
    </section>}
    {/* Oculta el listado al crear o editar; vuelve a mostrarlo al cerrar el formulario. */}
    {!formOpen && <PresentationList items={items} loading={loading} error={error} onReload={reload}
      onEdit={canEdit ? startEdit : undefined} onStateChange={canEdit ? changeState : undefined} pending={pending} editing={formOpen} />}
    {canEdit && !formOpen && <button ref={addButton} type="button" className="catalog-add-button"
      disabled={pending} aria-label="Crear presentación" title="Crear presentación" aria-expanded={formOpen}
      onClick={() => { setNotice(''); setFormOpen(true) }}>
      <span aria-hidden="true">+</span>
    </button>}
  </div>
}


