import { useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { useCategories } from '../../categories/hooks/useCategories'
import { useBrands } from '../../brands/hooks/useBrands'
import type { CategoryResponse } from '../../categories/types/category'
import type { BrandResponse } from '../../brands/types/brand'
import { ProductListPage } from './ProductListPage'
import { ProductSearch } from '../components/ProductSearch'
import './ProductsPage.css'

function CategoryCardImage({ url }: { url?: string | null }) {
  const [failed, setFailed] = useState(false)
  let valid = Boolean(url && /^\/uploads\/categorias\/[a-zA-Z0-9.-]+$/.test(url))
  if (url?.startsWith('https://')) {
    try { const parsed = new URL(url); valid = !parsed.username && !parsed.password && Boolean(parsed.hostname) }
    catch { valid = false }
  }
  return <span className="product-category-image">{valid && !failed
    ? <img src={url!} alt="" loading="lazy" referrerPolicy="no-referrer" onError={() => setFailed(true)} />
    : <span>Sin imagen</span>}</span>
}
export function ProductCatalogBrowser() {
  const { session } = useAuth()//comproba que el usuario sea admin
  const categories = useCategories()
  const brands = useBrands()//carga las categorias desde la api
  const [category, setCategory] = useState<CategoryResponse | null>(null)
  const [brand, setBrand] = useState<BrandResponse | null>(null)
  const [searchActive, setSearchActive] = useState(false)
  const title = useRef<HTMLHeadingElement>(null)
  useEffect(() => { title.current?.focus() }, [category, brand])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  const selectedBrands = brands.items.filter(item => item.idCategoria === category?.idCategoria && item.estado)
  return <div className="product-catalog-browser">
    <ProductSearch onResult={setSearchActive} />
    {!searchActive && <>
    <nav className="product-catalog-path" aria-label="Navegación de productos">
      <button type="button" onClick={() => { setCategory(null); setBrand(null) }} aria-current={!category ? 'page' : undefined}>Categorías</button>
      {category && <><span aria-hidden="true">›</span><button type="button" onClick={() => setBrand(null)} aria-current={!brand ? 'page' : undefined}>{category.nombre}</button></>}
      {brand && <><span aria-hidden="true">›</span><span aria-current="page">{brand.nombre}</span></>}
    </nav>
    <h2 ref={title} tabIndex={-1}>{brand ? `Productos de ${brand.nombre}` : category ? `Marcas de ${category.nombre}` : 'Selecciona una categoría'}</h2>
    {category && <button type="button" className="product-catalog-back" onClick={() => { if (brand) setBrand(null); else setCategory(null) }}>← {brand ? 'Volver a las marcas' : 'Volver a las categorías'}</button>}
    {!category && <>
      {categories.loading && <p role="status">Cargando categorías…</p>}
      {categories.error && <><p role="alert" className="product-error">{categories.error}</p><button type="button" onClick={categories.reload}>Reintentar</button></>}
      {!categories.loading && !categories.error && !categories.items.length && <p>No hay categorías disponibles.</p>}
      {!categories.loading && !categories.error && <ul className="product-catalog-grid">
        {categories.items.filter(item => item.estado).map(item => <li key={item.idCategoria}>
          <button type="button" className="product-category-card" onClick={() => setCategory(item)}>
            <CategoryCardImage key={item.urlImagen ?? ''} url={item.urlImagen} />
            <strong>{item.nombre}</strong>
          </button>
        </li>)}
      </ul>}
    </>}
    {category && !brand && <>
      {brands.loading && <p role="status">Cargando marcas…</p>}
      {brands.error && <><p role="alert" className="product-error">{brands.error}</p><button type="button" onClick={brands.reload}>Reintentar</button></>}
      {!brands.loading && !brands.error && !selectedBrands.length && <p>No hay marcas disponibles en esta categoría.</p>}
      {!brands.loading && !brands.error && <ul className="product-catalog-grid">
        {selectedBrands.map(item => <li key={item.idMarca}><button type="button" className="product-brand-card" onClick={() => setBrand(item)}><strong>{item.nombre}</strong><span>Ver productos →</span></button></li>)}
      </ul>}
    </>}
    {brand && category && <ProductListPage key={`${category.idCategoria}:${brand.idMarca}`} idMarca={brand.idMarca} idCategoria={category.idCategoria} />}
    </>}
  </div>
}