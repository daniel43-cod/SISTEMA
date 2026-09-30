import type { PresentationResponse } from '../types/presentation'

type Props = {
  items: PresentationResponse[]
  loading: boolean
  error: string
  onReload: () => void
}
export function PresentationList({ items, loading, error, onReload }: Props) {
  return <section className="presentation-card presentation-list" aria-labelledby="presentation-list-title" aria-busy={loading}>
    <div className="presentation-list-heading">
      <h2 id="presentation-list-title">Presentaciones activas</h2>
      <button type="button" className="presentation-refresh" disabled={loading} onClick={onReload}>Actualizar</button>
    </div>
    {loading ? <p role="status">Cargando presentaciones…</p> :
      error ? <p className="presentation-error" role="alert">{error}</p> :
      items.length === 0 ? <p className="presentation-description" role="status">No hay presentaciones activas.</p> :
      <>
        <p className="presentation-hint" role="status">{items.length} presentaciones activas</p>
        <ul className="presentation-list-items">
          {items.map(item => <li key={item.idPresentacion}>
            <strong>{item.descripcion}</strong><span className="presentation-active">Activa</span>
          </li>)}
        </ul>
      </>}
  </section>
}