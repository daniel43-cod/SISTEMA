import { useEffect, useRef, useState } from 'react'
import '../../../shared/ui/catalog.css'
import { useAuth } from '../../auth'
import { CategoryForm } from '../components/CategoryForm'
import { CategoryList } from '../components/CategoryList'
import { useCategories } from '../hooks/useCategories'
import type { CategoryResponse } from '../types/category'
import './CreateCategoryPage.css'

export function CreateCategoryPage() {
  const { session } = useAuth()
  const { items, loading, error, reload } = useCategories()
  const [editing, setEditing] = useState<CategoryResponse>()
  const [formOpen, setFormOpen] = useState(false)
  const addButton = useRef<HTMLButtonElement>(null)
  const [notice, setNotice] = useState('')
  const editor = useRef<HTMLElement>(null)
  const trigger = useRef<HTMLElement | null>(null)
  useEffect(() => {
    if (formOpen) {
      const panel = document.getElementById('catalog-editor')
      panel?.focus()
      panel?.scrollIntoView({ block: 'start', behavior: 'smooth' })
    }
  }, [formOpen])
  if (!session) return null
  const canEdit = session.user.role === 'ADMINISTRADOR'
  function closeEditor() {
    setFormOpen(false)
    setEditing(undefined)
    requestAnimationFrame(() => addButton.current?.focus())
  }
  return <div className="categories-page catalog-page">
    <p className="catalog-notice" role="status">{notice}</p>
    {canEdit && formOpen && <section id="catalog-editor" ref={editor} tabIndex={-1} className="category-card" aria-labelledby="create-category-title">
      <h2 id="create-category-title">{editing ? 'Editar categoría' : 'Crear categoría'}</h2>
      <button type="button" className="catalog-close" onClick={closeEditor}>Cerrar formulario</button>
      <CategoryForm key={editing?.idCategoria ?? 'new'} category={editing}
        onCancel={editing ? closeEditor : undefined}
        onSaved={() => {
          reload()
          setNotice(editing ? 'Categoría actualizada correctamente.' : 'Categoría creada correctamente.'); closeEditor()
        }} />
    </section>}
    {/* Oculta el listado al crear o editar; vuelve a mostrarlo al cerrar el formulario. */}
    {!formOpen && <CategoryList items={items} loading={loading} error={error} reload={reload} editing={formOpen}
      onEdit={canEdit ? item => {
        trigger.current = document.activeElement as HTMLElement
        setNotice(''); setEditing(item); setFormOpen(true)
        editor.current?.focus()
        editor.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
      } : undefined} />}
    {canEdit && !formOpen && <button ref={addButton} type="button" className="catalog-add-button"
      aria-label="Crear categoría" title="Crear categoría" aria-expanded={formOpen}
      onClick={() => { setNotice(''); setFormOpen(true) }}>
      <span aria-hidden="true">+</span>
    </button>}
  </div>
}


