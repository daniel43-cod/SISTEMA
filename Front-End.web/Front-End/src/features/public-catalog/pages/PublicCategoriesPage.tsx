import type { PublicCategory } from '../types/category'
import type { PublicBrand } from '../types/brand'
import { CatalogImage } from '../components/CatalogImage'
import { PublicBrandsList } from '../components/PublicBrandsList'
import { PublicProductsList } from '../components/PublicProductsList'
import { PublicCart } from '../components/PublicCart'
import { useEffect, useRef, useState } from 'react'
import { usePublicCategories } from '../hooks/usePublicCategories'
import { usePublicCart } from '../hooks/usePublicCart'
import './PublicCategoriesPage.css'

export function PublicCategoriesPage() {
  const { items, loading, error, reload } = usePublicCategories()
  const [selectedCategory, setSelectedCategory] = useState<PublicCategory | null>(null)
  const [selectedBrand, setSelectedBrand] = useState<PublicBrand | null>(null)
  const cart = usePublicCart()
  const headingRef = useRef<HTMLHeadingElement>(null)
  const navigationStarted = useRef(false)
  useEffect(() => {
    if (navigationStarted.current) headingRef.current?.focus()
    navigationStarted.current = true
  }, [selectedCategory, selectedBrand])
  useEffect(() => {
    const previous = document.title
    document.title = 'Catálogo | Sistema'
    return () => { document.title = previous }
  }, [])
  return <div className="public-catalog">
    <a className="public-catalog-skip" href="#public-categories">Ir al catálogo</a>
    <header className="public-catalog-header">
      <a className="public-catalog-brand" href={`${import.meta.env.BASE_URL}catalogo`}>
        <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} alt="" width="44" height="44" />
        <span>Sistema<small>CATÁLOGO PÚBLICO</small></span>
      </a>
      <a className="public-cart-link" href="#public-cart-title">Carrito ({cart.count})</a>
    </header>
    <main id="public-categories" className="public-catalog-main">
      <p className="public-catalog-eyebrow">EXPLORA NUESTROS PRODUCTOS</p>
      <h1 ref={headingRef} tabIndex={-1}>{selectedBrand ? `Productos de ${selectedBrand.nombre}`
        : selectedCategory ? `Marcas de ${selectedCategory.nombre}` : 'Categorías disponibles'}</h1>
      <p className="public-catalog-description">{selectedBrand
        ? 'Elige tus presentaciones y usa + para agregarlas al carrito.'
        : selectedCategory ? 'Conoce las marcas disponibles en esta categoría.'
        : 'Conoce nuestras categorías sin crear una cuenta.'}</p>
      {selectedCategory && <button type="button" className="public-catalog-back"
        onClick={() => {
          if (selectedBrand) setSelectedBrand(null)
          else setSelectedCategory(null)
        }}>{selectedBrand ? '← Volver a las marcas' : '← Volver a las categorías'}</button>}
      {selectedBrand
        ? <PublicProductsList key={selectedBrand.idMarca} idMarca={selectedBrand.idMarca}
            cartItems={cart.items} setQuantity={cart.setQuantity} />
        : selectedCategory
          ? <PublicBrandsList key={selectedCategory.idCategoria} idCategoria={selectedCategory.idCategoria}
              onSelect={setSelectedBrand} />
          : <>
            {loading && <p className="public-catalog-message" role="status">Cargando categorías…</p>}
            {!loading && error && <div className="public-catalog-message" role="alert">
              <p>{error}</p><button type="button" onClick={reload}>Reintentar</button>
            </div>}
            {!loading && !error && !items.length &&
              <p className="public-catalog-message" role="status">Por ahora no hay categorías disponibles.</p>}
            {!loading && !error && items.length > 0 && <ul className="public-categories-grid">
              {items.map(category => <li key={category.idCategoria} className="public-category-card">
                <button type="button" className="public-category-select" onClick={() => setSelectedCategory(category)}>
                  <CatalogImage key={category.urlImagen} url={category.urlImagen} name={category.nombre} kind="categorias" />
                  <h2>{category.nombre}</h2>
                </button>
              </li>)}
            </ul>}
          </>}
      <PublicCart items={cart.items} total={cart.total} count={cart.count} storageError={cart.storageError}
        setQuantity={cart.setQuantity} clear={cart.clear} />
    </main>
    <footer className="public-catalog-footer">Sistema · Catálogo público</footer>
  </div>
}
