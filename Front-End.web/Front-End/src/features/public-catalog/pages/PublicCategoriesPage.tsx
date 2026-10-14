import type { PublicCategory } from '../types/category'
import type { PublicBrand } from '../types/brand'
import { CatalogImage } from '../components/CatalogImage'
import { PublicBrandsList } from '../components/PublicBrandsList'
import { PublicProductsList } from '../components/PublicProductsList'
import { FloatingPublicCart } from '../components/FloatingPublicCart'
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
        <span>Distrbuidora San Antonio<small>CATÁLOGO</small></span>
      </a>

    </header>
    <main id="public-categories" className="public-catalog-main">
      <div className={selectedCategory ? 'public-catalog-heading public-catalog-heading-sticky' : 'public-catalog-heading'}>
        <p className="public-catalog-eyebrow">EXPLORA NUESTROS PRODUCTOS</p>
        <div className="public-catalog-title-row">
          {selectedCategory && <button type="button" className="public-catalog-back-icon"
            aria-label={selectedBrand ? 'Volver a las marcas' : 'Volver a las categorías'}
            title={selectedBrand ? 'Volver a las marcas' : 'Volver a las categorías'}
            onClick={() => {
              if (selectedBrand) setSelectedBrand(null)
              else setSelectedCategory(null)
            }}>
            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
              strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="m12 19-7-7 7-7M5 12h14" />
            </svg>
          </button>}
          <h1 ref={headingRef} tabIndex={-1}>{selectedBrand ? `Productos de ${selectedBrand.nombre}`
            : selectedCategory ? `Marcas de ${selectedCategory.nombre}` : 'Categorías disponibles'}</h1>
        </div>
      </div>
      {!selectedBrand && <p className="public-catalog-description">{selectedCategory
        ? 'Conoce las marcas disponibles en esta categoría.'
        : 'Conoce nuestras categorías sin crear una cuenta.'}</p>}

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
      <FloatingPublicCart items={cart.items} total={cart.total} count={cart.count} storageError={cart.storageError}
        setQuantity={cart.setQuantity} clear={cart.clear} />
    </main>
    <footer className="public-catalog-footer">Sistema · Catálogo público</footer>
  </div>
}
