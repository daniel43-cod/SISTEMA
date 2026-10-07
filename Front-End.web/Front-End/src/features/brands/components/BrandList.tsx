import { RowDetails } from '../../../shared/ui/RowDetails'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import type { BrandResponse } from '../types/brand'

// Recibe los datos del hook: muestra el listado y los estados de carga, error y vacío.
export function BrandList({ items, loading, error, reload, onEdit, onStateChange, pending = false }: {
  items: BrandResponse[]; loading: boolean; error: string; reload: () => void; onEdit?: (brand: BrandResponse) => void; onStateChange?: (brand: BrandResponse) => void; pending?: boolean
}) {
  return <section className="brand-card" aria-labelledby="brand-list-title" aria-busy={loading || pending}>
    <div className="catalog-list-heading">
      <h2 id="brand-list-title">Marcas</h2>
      <RefreshButton onClick={reload} loading={loading || pending} label="Actualizar marcas" />
    </div>
 
    {loading && <p role="status">Cargando marcas…</p>}
    {error && <p className="brand-error" role="alert">{error}</p>}
    {!loading && !error && items.length === 0 && <p>No hay marcas para mostrar. Usa el botón + para crear una.</p>}
    <ul className="brand-list">
      {items.map(item => <li key={item.idMarca} className="catalog-clickable-row">
        <RowDetails name={item.nombre} type="marca" fields={[{ label: 'Categoría', value: item.nombreCategoria || 'No disponible' }, { label: 'Estado', value: item.estado ? 'Activo' : 'Inactivo' }]} image={item.urlImagen ?? null} disabled={loading || pending} />
                {/* nombre es la marca; nombreCategoria viene de su relación en la API. */}
        <div><strong>{item.nombre}</strong><span>Categoría: {item.nombreCategoria || 'No disponible'}</span></div>
        <div className="brand-row-actions">
        {onEdit && <button type="button" className="brand-refresh" disabled={!item.estado || loading || pending}
          aria-label={"Editar " + item.nombre} onClick={() => onEdit(item)}>Editar</button>}
        {onStateChange && <button type="button"
          className={`brand-state-button ${item.estado ? 'brand-state-active' : 'brand-state-inactive'}`}
          disabled={loading || pending} aria-pressed={item.estado}
          title={item.estado ? 'Desactivar marca' : 'Activar marca'}
          aria-label={`${item.estado ? 'Activo' : 'Inactivo'}: ${item.nombre}. ${item.estado ? 'Desactivar' : 'Activar'} marca`}
          onClick={() => onStateChange(item)}>{item.estado ? 'Activo' : 'Inactivo'}</button>}
        </div>
      </li>)}
    </ul>
  </section>
}





