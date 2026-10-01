import { useRef, useState } from 'react'
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
  const [notice, setNotice] = useState('')
  const editorRef = useRef<HTMLElement>(null)
  const editTrigger = useRef<HTMLElement | null>(null)
  if (!session) return null
  const canEdit = session.user.role === 'ADMINISTRADOR'
  function startEdit(item: PresentationResponse) {
    editTrigger.current = document.activeElement instanceof HTMLElement ? document.activeElement : null
    setNotice('')
    setEditing(item)
    editorRef.current?.focus()
    editorRef.current?.scrollIntoView({ block: 'nearest', behavior: 'instant' })
  }
  return <div className="presentations-page">
    {canEdit && <section className="presentation-card" aria-labelledby="presentation-form-title" ref={editorRef} tabIndex={-1}>
      <span className="presentation-label">Presentaciones</span>
      <h2 id="presentation-form-title">{editing ? 'Editar presentación' : 'Crear presentación'}</h2>
      
      <p className="presentation-success" role="status">{notice}</p>
      {editing ? <EditPresentationForm key={editing.idPresentacion} presentation={editing}
        onCancel={() => { setEditing(null); editTrigger.current?.focus() }}
        onSaved={() => { setEditing(null); setNotice('Presentación actualizada correctamente.'); reload(); editorRef.current?.focus() }} />
        : <PresentationForm onCreated={reload} />}
    </section>}
    <PresentationList items={items} loading={loading} error={error} onReload={reload}
      onEdit={canEdit ? startEdit : undefined} editing={editing !== null} />
  </div>
}