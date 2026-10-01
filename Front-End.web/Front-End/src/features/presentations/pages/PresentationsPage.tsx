import { useEffect, useRef, useState } from 'react'
import '../../../shared/ui/catalog.css'
import { useAuth } from '../../auth'
import { PresentationForm } from '../components/PresentationForm'
import { EditPresentationForm } from '../components/EditPresentationForm'
import { PresentationList } from '../components/PresentationList'
import { usePresentations } from '../hooks/usePresentations'
import type { PresentationResponse } from '../types/presentation'
import './PresentationsPage.css'

export function PresentationsPage() {
  const { session } = useAuth()
  const { items, loading, error, reload } = usePresentations()
  const [editing, setEditing] = useState<PresentationResponse | null>(null)
  const [formOpen, setFormOpen] = useState(false)
  const addButton = useRef<HTMLButtonElement>(null)
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
      onEdit={canEdit ? startEdit : undefined} editing={formOpen} />}
    {canEdit && !formOpen && <button ref={addButton} type="button" className="catalog-add-button"
      aria-label="Crear presentación" title="Crear presentación" aria-expanded={formOpen}
      onClick={() => { setNotice(''); setFormOpen(true) }}>
      <span aria-hidden="true">+</span>
    </button>}
  </div>
}


