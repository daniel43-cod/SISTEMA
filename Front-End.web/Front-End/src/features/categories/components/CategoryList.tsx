import { useState } from 'react'
import type { CategoryResponse } from '../types/category'

function CategoryImage({ url }: { url?: string | null }) {
  const [failed, setFailed] = useState(false)
  const valid = url && (/^https:\/\//i.test(url) || /^\/uploads\/categorias\/[a-zA-Z0-9.-]+$/.test(url))
  return valid && !failed
    ? <img className="category-thumbnail" src={url} alt="" loading="lazy" referrerPolicy="no-referrer" onError={() => setFailed(true)} />
    : <span className="category-thumbnail category-thumbnail-empty">Sin imagen</span>
}
export function CategoryList({ items, loading, error, reload, onEdit, editing }: {
  items: CategoryResponse[]; loading: boolean; error: string; reload: () => void
  onEdit?: (item: CategoryResponse) => void; editing: boolean
}) {
  return <section className="category-card" aria-labelledby="category-list-title" aria-busy={loading}>
    <div className="category-list-heading">
      <h2 id="category-list-title">Categorías</h2>
      <button type="button" className="category-list-action" onClick={reload} disabled={loading}aria-label='Actualizar lista de categorias' title='Actualizar lista'>
        <svg
        width="25"
        height="25"
        viewBox='0 0 24 24'
        fill='none'
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true">
      <path d="M20 7v5h-5" />
    <path d="M20 12a8 8 0 1 0-2.3 5.7" />
    </svg>
      </button>
    </div>
    {loading && <p role="status">Cargando categorías…</p>}
    {error && <p className="category-error" role="alert">{error}</p>}
    {!loading && !error && !items.length && <p>No hay categorías activas.</p>}
    <ul className="category-list">
      {items.map(item => <li key={item.idCategoria}>
        <CategoryImage key={item.urlImagen ?? ''} url={item.urlImagen} />
        <strong>{item.nombre}</strong>
        {onEdit && <button type="button" className="category-list-action" disabled={editing || loading}
          aria-label={'Editar ' + item.nombre} onClick={() => onEdit(item)}>Editar</button>}
      </li>)}
    </ul>
  </section>
}
