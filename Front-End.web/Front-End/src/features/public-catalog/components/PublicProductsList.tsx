import { usePublicProducts } from '../hooks/usePublicProducts'
import { CatalogImage } from './CatalogImage'
import { MAX_QUANTITY } from '../utils/cart'
import type { CartItem } from '../types/product'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function PublicProductsList({ idMarca, cartItems, setQuantity }: {
  idMarca: number
  cartItems: CartItem[]
  setQuantity: (item: Omit<CartItem, 'cantidad'>, cantidad: number) => void
}) {
  const { items, pagina, hayMas, loading, error, reload, setPagina } = usePublicProducts(idMarca)
  return <>
    {loading && <p className="public-catalog-message" role="status">Cargando productos…</p>}
    {!loading && error && <div className="public-catalog-message" role="alert">
      <p>{error}</p><button type="button" onClick={reload}>Reintentar</button>
    </div>}
    {!loading && !error && !items.length && <p className="public-catalog-message" role="status">
      No hay productos disponibles en esta página.
    </p>}
    {!loading && !error && items.length > 0 && <ul className="public-products-grid">
      {items.map(product => <li key={product.idProducto} className="public-category-card">
        <CatalogImage key={product.urlImagen} url={product.urlImagen} name={product.nombre} kind="productos" />
        <h2>{product.nombre}</h2>
        <ul className="public-presentations">
          {product.presentaciones.map(presentation => {
            const quantity = cartItems.find(row => row.idProductoPresentacion === presentation.idProductoPresentacion)?.cantidad ?? 0
            const cartItem = { idProducto: product.idProducto, idProductoPresentacion: presentation.idProductoPresentacion,
              producto: product.nombre, presentacion: presentation.nombre, precio: presentation.precio }
            const label = `${product.nombre}, ${presentation.nombre}`
            return <li key={presentation.idProductoPresentacion} className="public-presentation">
              <div><strong>{presentation.nombre}</strong><span>{money.format(presentation.precio)}</span>
                <small>{presentation.disponible ? 'Disponible' : 'Agotado'}</small></div>
              <div className="public-quantity" role="group" aria-label={`Cantidad en carrito de ${label}`}>
                <button type="button" aria-label={`Quitar uno: ${label}`} disabled={quantity === 0}
                  onClick={() => setQuantity(cartItem, quantity - 1)}>−</button>
                <output aria-label={`Cantidad de ${label}`}>{quantity}</output>
                <button type="button" aria-label={`Agregar uno: ${label}`}
                  disabled={!presentation.disponible || quantity >= MAX_QUANTITY}
                  onClick={() => setQuantity(cartItem, quantity + 1)}>+</button>
              </div>
            </li>
          })}
        </ul>
      </li>)}
    </ul>}
    <nav className="public-pagination" aria-label="Páginas de productos">
      <button type="button" disabled={loading || pagina === 1} onClick={() => setPagina(pagina - 1)}>Anterior</button>
      <span>Página {pagina}</span>
      <button type="button" disabled={loading || !!error || !hayMas} onClick={() => setPagina(pagina + 1)}>Siguiente</button>
    </nav>
  </>
}
