import { usePublicProducts } from '../hooks/usePublicProducts'
import { CatalogProductCard } from './CatalogProductCard'
import { MAX_QUANTITY } from '../utils/cart'
import { CatalogSurtidoCard } from './CatalogSurtidoCard'
import type { CartItem } from '../types/product'


export function PublicProductsList({ idMarca, cartItems, setQuantity }: {
  idMarca: number
  cartItems: CartItem[]
  setQuantity: (item: Omit<CartItem, 'cantidad'>, cantidad: number) => void
}) {
  const { items, surtidos, pagina, hayMas, loading, error, reload, setPagina } = usePublicProducts(idMarca)
  return <>
    {loading && <p className="public-catalog-message" role="status">Cargando productos…</p>}
    {!loading && error && <div className="public-catalog-message" role="alert">
      <p>{error}</p><button type="button" onClick={reload}>Reintentar</button>
    </div>}
    {!loading && !error && !items.length && !surtidos.length && <p className="public-catalog-message" role="status">
      No hay productos disponibles en esta página.
    </p>}
    {!loading && !error && (items.length > 0 || surtidos.length > 0) && <ul className="public-products-grid">
      {surtidos.map(product => <CatalogSurtidoCard key={`surtido-${product.idSurtido}`} product={product} actionLabel="Agregar al carrito"
        initial={cartItems.filter(i => i.idSurtido === product.idSurtido).map(i => ({ idProductoPresentacion: i.idProductoPresentacion, cantidad: i.cantidad,
          unidadesEquivalentes: i.unidadesEquivalentes!, componentes: i.componentes! }))}
        onAdd={selections => {
          if (cartItems.length + selections.filter(s => !cartItems.some(i => i.idProductoPresentacion === s.idProductoPresentacion)).length > 100)
            return 'El pedido admite hasta 100 presentaciones.'
          for (const s of selections) {
            const p = product.presentaciones.find(p => p.idProductoPresentacion === s.idProductoPresentacion)!
            setQuantity({ idProducto: 0, idSurtido: product.idSurtido, idProductoPresentacion: s.idProductoPresentacion, producto: product.nombre,
              presentacion: p.nombre, precio: p.precio, unidadesEquivalentes: s.unidadesEquivalentes, componentes: s.componentes }, s.cantidad)
          }
          return null
        }} />)}
      {items.map(product => <CatalogProductCard key={product.idProducto} product={product} renderActions={presentation => {
        const quantity = cartItems.find(row => row.idProductoPresentacion === presentation.idProductoPresentacion)?.cantidad ?? 0
        const cartItem = { idProducto: product.idProducto, idProductoPresentacion: presentation.idProductoPresentacion,
          producto: product.nombre, presentacion: presentation.nombre, precio: presentation.precio }
        const label = product.nombre + ', ' + presentation.nombre
        return <div className="public-quantity" role="group" aria-label={'Cantidad en carrito de ' + label}>
          <button type="button" aria-label={'Quitar uno: ' + label} disabled={quantity === 0}
            onClick={() => setQuantity(cartItem, quantity - 1)}>−</button>
          <output aria-label={'Cantidad de ' + label}>{quantity}</output>
          <button type="button" aria-label={'Agregar uno: ' + label}
            disabled={!presentation.disponible || quantity >= MAX_QUANTITY}
            onClick={() => setQuantity(cartItem, quantity + 1)}>+</button>
        </div>
      }} />)}
    </ul>}
    <nav className="public-pagination" aria-label="Páginas de productos">
      <button type="button" disabled={loading || pagina === 1} onClick={() => setPagina(pagina - 1)}>Anterior</button>
      <span>Página {pagina}</span>
      <button type="button" disabled={loading || !!error || !hayMas} onClick={() => setPagina(pagina + 1)}>Siguiente</button>
    </nav>
  </>
}
