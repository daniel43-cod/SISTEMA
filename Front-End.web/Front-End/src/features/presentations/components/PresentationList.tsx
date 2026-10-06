import { RefreshButton } from '../../../shared/ui/RefreshButton'
import type { PresentationResponse } from '../types/presentation'

type Props = {
  items: PresentationResponse[]
  loading: boolean
  error: string
  onReload: () => void
  onEdit?: (item: PresentationResponse) => void
  onStateChange?: (item: PresentationResponse) => void
  pending?: boolean
  editing: boolean
}
export function PresentationList({ items, loading, error, onReload, onEdit, onStateChange, pending = false, editing }: Props) {
  return <section className="presentation-card presentation-list" aria-labelledby="presentation-list-title" aria-busy={loading}>
    <div className="catalog-list-heading">
      <h2 id="presentation-list-title">{onStateChange ? 'Presentaciones' : 'Presentaciones activas'}</h2>
      <RefreshButton onClick={onReload} loading={loading || pending} label="Actualizar presentaciones" />
    </div>
    {loading ? <p role="status">Cargando presentaciones…</p> :
      error ? <p className="presentation-error" role="alert">{error}</p> :
      items.length === 0 ? <p className="presentation-description" role="status">No hay presentaciones para mostrar.</p> :
      <>
        <p className="presentation-hint" role="status">{items.length} presentaciones</p>
        <ul className="presentation-list-items">
          {items.map(item => <li key={item.idPresentacion}>
                        <strong>{item.descripcion}</strong>
            <div className="presentation-row-actions">

              {onEdit && <button type="button" className="presentation-edit-button" disabled={editing || pending}
                aria-label={'Editar ' + item.descripcion} onClick={() => onEdit(item)}>Editar</button>}
              {onStateChange && <button type="button" className={`presentation-state-button ${item.estado ? 'presentation-state-active' : 'presentation-state-inactive'}`} disabled={editing || pending || loading}
                aria-pressed={item.estado} title={item.estado ? 'Desactivar presentación' : 'Activar presentación'}
                aria-label={`${item.estado ? 'Activo' : 'Inactivo'}: ${item.descripcion}. ${item.estado ? 'Desactivar' : 'Activar'} presentación`}
                onClick={() => onStateChange(item)}>{item.estado ? 'Activo' : 'Inactivo'}</button>}
            </div>
          </li>)}
        </ul>
      </>}
  </section>
}

