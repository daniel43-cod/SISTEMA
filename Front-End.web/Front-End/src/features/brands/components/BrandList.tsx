import { RefreshButton } from '../../../shared/ui/RefreshButton'
import type { BrandResponse } from '../types/brand'

// Recibe los datos del hook: muestra el listado y los estados de carga, error y vacío.
export function BrandList({ items, loading, error, reload, onEdit }: {
  items: BrandResponse[]; loading: boolean; error: string; reload: () => void; onEdit?: (brand: BrandResponse) => void
}) {
  return <section className="brand-card" aria-labelledby="brand-list-title" aria-busy={loading}>
    <div className="catalog-list-heading">
      <h2 id="brand-list-title">Marcas</h2>
      <RefreshButton onClick={reload} loading={loading} label="Actualizar marcas" />
    </div>
 
    {loading && <p role="status">Cargando marcas…</p>}
    {error && <p className="brand-error" role="alert">{error}</p>}
    {!loading && !error && items.length === 0 && <p>No hay marcas activas. Usa el botón + para crear una.</p>}
    <ul className="brand-list">
      {items.map(item => <li key={item.idMarca}>
                {/* nombre es la marca; nombreCategoria viene de su relación en la API. */}
        <div><strong>{item.nombre}</strong><span>Categoría: {item.nombreCategoria || 'No disponible'}</span></div>
        <span className="brand-active">Activa</span>
        {onEdit && <button type="button" className="brand-refresh" disabled={loading}
          aria-label={"Editar " + item.nombre} onClick={() => onEdit(item)}>Editar</button>}
      </li>)}
    </ul>
  </section>
}



