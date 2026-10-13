import type { PublicBrand } from '../types/brand'
import { usePublicBrands } from '../hooks/usePublicBrands'
import { CatalogImage } from './CatalogImage'

export function PublicBrandsList({ idCategoria, onSelect }: { idCategoria: number; onSelect: (brand: PublicBrand) => void }) {
  const { items, loading, error, reload } = usePublicBrands(idCategoria)
  if (loading) return <p className="public-catalog-message" role="status">Cargando marcas…</p>
  if (error) return <div className="public-catalog-message" role="alert">
    <p>{error}</p><button type="button" onClick={reload}>Reintentar</button>
  </div>
  if (!items.length) return <p className="public-catalog-message" role="status">
    Por ahora no hay marcas disponibles en esta categoría.
  </p>
  return <ul className="public-categories-grid" aria-label="Marcas disponibles">
    {items.map(brand => <li key={brand.idMarca} className="public-category-card">
      <button type="button" className="public-category-select" onClick={() => onSelect(brand)}><CatalogImage key={brand.urlImagen} url={brand.urlImagen} name={brand.nombre} kind="marcas" />
      <h2>{brand.nombre}</h2></button>
    </li>)}
  </ul>
}
