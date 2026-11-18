import type { PublicCategory } from '../types/category'
import { CatalogImage } from './CatalogImage'
export function CatalogCategoriesList({ items, onSelect }: {
  items: PublicCategory[]; onSelect: (category: PublicCategory) => void
}) {
  return <ul className="public-categories-grid">
    {items.map(category => <li key={category.idCategoria} className="public-category-card">
      <button type="button" className="public-category-select" onClick={() => onSelect(category)}>
        <CatalogImage key={category.urlImagen} url={category.urlImagen} name={category.nombre} kind="categorias" />
        <h2>{category.nombre}</h2>
      </button>
    </li>)}
  </ul>
}