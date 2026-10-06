import { changeCategoryState } from '../api/categoriesApi'
import { ApiError } from '../../../shared/api/ApiError'
import { useEffect, useRef, useState } from 'react'
import '../../../shared/ui/catalog.css'
import { useAuth } from '../../auth'
import { CategoryForm } from '../components/CategoryForm'
import { CategoryList } from '../components/CategoryList'
import { useCategories } from '../hooks/useCategories'
import type { CategoryResponse } from '../types/category'
import './CreateCategoryPage.css'

export function CreateCategoryPage() {
  const { session, logout } = useAuth()
  const { items, loading, error, reload } = useCategories(session?.user.role === 'ADMINISTRADOR')
  const [editing, setEditing] = useState<CategoryResponse>()
  const [formOpen, setFormOpen] = useState(false)
  const [pending, setPending] = useState(false)
  const [stateError, setStateError] = useState('')
  const stateRequest = useRef<AbortController | null>(null)
  useEffect(() => () => stateRequest.current?.abort(), [])
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
  async function changeState(item: CategoryResponse) {
    if (!session || !canEdit || stateRequest.current) return
    const controller = new AbortController()
    stateRequest.current = controller
    setPending(true); setStateError(''); setNotice('')
    try {
      await changeCategoryState(item.idCategoria, !item.estado, session.token, controller.signal)
      if (!controller.signal.aborted) { setNotice(item.estado ? 'Categoría desactivada.' : 'Categoría activada.'); reload() }
    } catch (failure) {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setStateError(failure instanceof ApiError ? failure.message : 'No se pudo cambiar el estado.')
    } finally {
      stateRequest.current = null
      if (!controller.signal.aborted) setPending(false)
    }
  }
  function closeEditor() {
    setFormOpen(false)
    setEditing(undefined)
    requestAnimationFrame(() => addButton.current?.focus())
  }
  return <div className="categories-page catalog-page">
    <p className="catalog-notice" role="status">{notice}</p>
    {stateError && <p className="category-error" role="alert">{stateError}</p>}
    {canEdit && formOpen && <section id="catalog-editor" ref={editor} tabIndex={-1} className="category-card catalog-editor" aria-labelledby="create-category-title">
      <h2 id="create-category-title">{editing ? 'Editar categoría' : 'Crear categoría'}</h2>
      <button type="button" className="catalog-close" aria-label="Cerrar formulario" title="Cerrar formulario" onClick={closeEditor}><span aria-hidden="true">×</span></button>
      <CategoryForm key={editing?.idCategoria ?? 'new'} category={editing}
        onCancel={editing ? closeEditor : undefined}
        onSaved={() => {
          reload()
          setNotice(editing ? 'Categoría actualizada correctamente.' : 'Categoría creada correctamente.'); closeEditor()
        }} />
    </section>}
    {/* Oculta el listado al crear o editar; vuelve a mostrarlo al cerrar el formulario. */}
    {!formOpen && <CategoryList items={items} loading={loading} error={error} reload={reload} editing={formOpen} pending={pending} onStateChange={canEdit ? changeState : undefined}
      onEdit={canEdit ? item => {
        trigger.current = document.activeElement as HTMLElement
        setNotice(''); setEditing(item); setFormOpen(true)
        editor.current?.focus()
        editor.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })
      } : undefined} />}
    {canEdit && !formOpen && <button ref={addButton} type="button" className="catalog-add-button"
      disabled={pending || loading} aria-label="Crear categoría" title="Crear categoría" aria-expanded={formOpen}
      onClick={() => { setNotice(''); setFormOpen(true) }}>
      <span aria-hidden="true">+</span>
    </button>}
  </div>
}




