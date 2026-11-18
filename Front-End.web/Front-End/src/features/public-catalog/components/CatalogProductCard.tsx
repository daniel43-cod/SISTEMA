import type { ReactNode } from 'react'
import type { PublicProduct, PublicPresentation } from '../types/product'
import { CatalogImage } from './CatalogImage'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function CatalogProductCard({ product, renderActions, footer }: {
  footer?: ReactNode
  product: PublicProduct
  renderActions: (presentation: PublicPresentation) => ReactNode
}) {
  return <li className="public-category-card">
    <CatalogImage key={product.urlImagen} url={product.urlImagen} name={product.nombre} kind="productos" />
    <h2>{product.nombre}</h2>
    <ul className="public-presentations">
      {product.presentaciones.map(presentation => <li key={presentation.idProductoPresentacion} className="public-presentation">
        <div className="public-presentation-info">
          <div className="public-presentation-heading">
            <strong>{presentation.nombre}</strong><span>{money.format(presentation.precio)}</span>
          </div>
          <small className={presentation.disponible ? 'public-availability-available' : 'public-availability-unavailable'}>
            {presentation.disponible ? 'Disponible' : 'Agotado'}
          </small>
        </div>
        {renderActions(presentation)}
      </li>)}
    </ul>
    {footer}
  </li>
}