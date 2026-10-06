import { useCallback, useEffect, useRef, useState } from 'react'
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
  const [namesRevision, setNamesRevision] = useState(0)
  const refreshNames = useCallback(() => setNamesRevision(value => value + 1), [])
  const [searchActive, setSearchActive] = useState(false)
  const title = useRef<HTMLHeadingElement>(null)
  useEffect(() => { title.current?.focus() }, [category, brand])
  if (session?.user.role !== 'ADMINISTRADOR') return null
  const selectedBrands = brands.items.filter(item => item.idCategoria === category?.idCategoria && item.estado)
  return <div className="product-catalog-browser">
    <ProductSearch onResult={setSearchActive} namesRevision={namesRevision} onNamesRefresh={refreshNames}
      onBack={category ? () => { if (brand) setBrand(null); else setCategory(null) } : undefined}
      backLabel={brand ? 'Volver a las marcas' : 'Volver a las categorías'} />
    {!searchActive && <>
    <h2 ref={title} tabIndex={-1} className={category ? 'product-navigation-title' : undefined}>{category ? (brand ? 'Productos' : 'Marcas') : 'Categorías'}</h2>
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
        {selectedBrands.map(item => <li key={item.idMarca}><button type="button" className="product-brand-card" onClick={() => setBrand(item)}><span className="product-brand-emblem" aria-hidden="true">{item.nombre.trim().slice(0, 2).toLocaleUpperCase('es')}</span>
            <strong>{item.nombre}</strong>
            <span className="product-brand-category">{category.nombre}</span></button></li>)}
      </ul>}
    </>}
    {brand && category && <ProductListPage key={`${category.idCategoria}:${brand.idMarca}`} idMarca={brand.idMarca} idCategoria={category.idCategoria} onProductSaved={refreshNames} />}
    </>}
  </div>
}