import { useRef, useState } from 'react'
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
  const [notice, setNotice] = useState('')
  const editor = useRef<HTMLElement>(null)
  const trigger = useRef<HTMLElement | null>(null)
  if (!session) return null
  const canEdit = session.user.role === 'ADMINISTRADOR'
  function closeEditor() {
    setEditing(undefined)
    requestAnimationFrame(() => trigger.current?.focus())
  }
  return <div className="categories-page">
    {canEdit && <section ref={editor} tabIndex={-1} className="category-card" aria-labelledby="create-category-title">
      <h2 id="create-category-title">{editing ? 'Editar categoría' : 'Crear categoría'}</h2>
      <p role="status">{notice}</p>
      <CategoryForm key={editing?.idCategoria ?? 'new'} category={editing}
        onCancel={editing ? closeEditor : undefined}
        onSaved={() => {
          reload()
          if (editing) { setNotice('Categoría actualizada correctamente.'); closeEditor() }
        }} />
    </section>}
    <CategoryList items={items} loading={loading} error={error} reload={reload} editing={!!editing}
      onEdit={canEdit ? item => {
        trigger.current = document.activeElement as HTMLElement
        setNotice(''); setEditing(item)
        editor.current?.focus()
        editor.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
      } : undefined} />
  </div>
}
