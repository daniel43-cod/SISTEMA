import '../../../shared/ui/mobile-catalog-cards.css'
import type { ReactNode } from 'react'
﻿import { RowDetails } from '../../../shared/ui/RowDetails'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import type { PresentationResponse } from '../types/presentation'

type Props = {
  createAction?: ReactNode
  items: PresentationResponse[]
  loading: boolean
  error: string
  onReload: () => void
  onEdit?: (item: PresentationResponse) => void
  onStateChange?: (item: PresentationResponse) => void
  pending?: boolean
  editing: boolean
}
export function PresentationList({ items, loading, error, onReload, onEdit, onStateChange, pending = false, editing, createAction }: Props) {
  return <section className="presentation-card presentation-list" aria-labelledby="presentation-list-title" aria-busy={loading}>
    <div className="catalog-list-heading">
      <h2 id="presentation-list-title">{onStateChange ? 'Presentaciones' : 'Presentaciones activas'}</h2>
      <div className="catalog-panel-actions"><RefreshButton onClick={onReload} loading={loading || pending} label="Actualizar presentaciones" />{createAction}</div>
    </div>
    {loading ? <p role="status">Cargando presentaciones…</p> :
      error ? <p className="presentation-error" role="alert">{error}</p> :
      items.length === 0 ? <p className="presentation-description" role="status">No hay presentaciones para mostrar.</p> :
      <>
        <p className="presentation-hint" role="status">{items.length} presentaciones</p>
    <ul className="mobile-catalog-cards" aria-label="Presentaciones">
      {items.map(item => <li key={item.idPresentacion}>
        <div className="mobile-catalog-card-info">
          <strong>{item.descripcion}</strong>
          <span className={item.estado ? 'mobile-catalog-status is-active' : 'mobile-catalog-status is-inactive'}>{item.estado ? 'Activo' : 'Inactivo'}</span>
        </div>
        <RowDetails compact name={item.descripcion} type="presentación"  disabled={editing || loading || pending}
          fields={[{ label: 'Estado', value: item.estado ? 'Activo' : 'Inactivo' }]}
          onEdit={onEdit ? () => onEdit(item) : undefined} editDisabled={!item.estado}
          actions={onStateChange && <button type="button" className={item.estado ? 'catalog-detail-deactivate' : 'catalog-detail-activate'} disabled={editing || loading || pending} onClick={() => onStateChange(item)}>{item.estado ? 'Desactivar presentación' : 'Activar presentación'}</button>} />
      </li>)}
    </ul>
        <ul className="presentation-list-items">
          {items.map(item => <li key={item.idPresentacion} className="catalog-clickable-row">
        <RowDetails name={item.descripcion} type="presentación" fields={[{ label: 'Estado', value: item.estado ? 'Activo' : 'Inactivo' }]}  disabled={editing || loading || pending} />
                        <strong>{item.descripcion}</strong>
            <div className="presentation-row-actions">

              {onEdit && <button type="button" className="presentation-edit-button" disabled={!item.estado || editing || pending}
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


