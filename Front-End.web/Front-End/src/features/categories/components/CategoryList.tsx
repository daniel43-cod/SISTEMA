import { RowDetails } from '../../../shared/ui/RowDetails'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { useState } from 'react'
import type { CategoryResponse } from '../types/category'

function CategoryImage({ url }: { url?: string | null }) {
  const [failed, setFailed] = useState(false)
  const valid = url && (/^https:\/\//i.test(url) || /^\/uploads\/categorias\/[a-zA-Z0-9.-]+$/.test(url))
  return valid && !failed
    ? <img className="category-thumbnail" src={url} alt="" loading="lazy" referrerPolicy="no-referrer" onError={() => setFailed(true)} />
    : <span className="category-thumbnail category-thumbnail-empty">Sin imagen</span>
}
export function CategoryList({ items, loading, error, reload, onEdit, editing, onStateChange, pending = false }: {
  items: CategoryResponse[]; loading: boolean; error: string; reload: () => void
  onEdit?: (item: CategoryResponse) => void; editing: boolean; onStateChange?: (item: CategoryResponse) => void; pending?: boolean
}) {
  return <section className="category-card" aria-labelledby="category-list-title" aria-busy={loading || pending}>
    <div className="catalog-list-heading">
      <h2 id="category-list-title">Categorías</h2>
      <RefreshButton onClick={reload} loading={loading || pending} label="Actualizar categorías" />
    </div>
    {loading && <p role="status">Cargando categorías…</p>}
    {error && <p className="category-error" role="alert">{error}</p>}
    {!loading && !error && !items.length && <p>No hay categorías para mostrar.</p>}
    <ul className="category-list">
      {items.map(item => <li key={item.idCategoria} className="catalog-clickable-row">
        <RowDetails name={item.nombre} type="categoría" fields={[{ label: 'Estado', value: item.estado ? 'Activo' : 'Inactivo' }]} image={item.urlImagen ?? null} disabled={editing || loading || pending} />
        <CategoryImage key={item.urlImagen ?? ''} url={item.urlImagen} />
        <strong>{item.nombre}</strong>
        <div className="category-row-actions">
        {onEdit && <button type="button" className="category-list-action" disabled={!item.estado || editing || loading || pending}
          aria-label={'Editar ' + item.nombre} onClick={() => onEdit(item)}>Editar</button>}
        {onStateChange && <button type="button"
          className={`category-state-button ${item.estado ? 'category-state-active' : 'category-state-inactive'}`}
          disabled={editing || loading || pending} aria-pressed={item.estado}
          title={item.estado ? 'Desactivar categoría' : 'Activar categoría'}
          aria-label={`${item.estado ? 'Activo' : 'Inactivo'}: ${item.nombre}. ${item.estado ? 'Desactivar' : 'Activar'} categoría`}
          onClick={() => onStateChange(item)}>{item.estado ? 'Activo' : 'Inactivo'}</button>}
        </div>
      </li>)}
    </ul>
  </section>
}



