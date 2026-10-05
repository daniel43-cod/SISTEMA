import { useState } from 'react'
import { useAuth } from '../../features/auth'
import { PresentationsPage } from '../../features/presentations'
import { CreateCategoryPage } from '../../features/categories'
import { BrandsPage } from '../../features/brands'
import { CreateProductPage } from '../../features/products'
import './ProductsPage.css'

export function ProductsPage() {
  const { session } = useAuth()
  //formulario inicial
  const [section, setSection] = useState<'presentations' | 'categories' | 'brands' | 'products'>('products')
  if (!session) return null
  const canCreate = session.user.role === 'ADMINISTRADOR'
  const showCategories = canCreate && section === 'categories'

  return <div>
    {canCreate && <nav className="products-sections" aria-label="Catálogos de productos">
       <button type="button" aria-current={section === "products" ? "page" : undefined}
        onClick={() => setSection("products")}>Productos</button>
     <button type="button" aria-current={section === "brands" ? "page" : undefined}
        onClick={() => setSection("brands")}>Marcas</button>
      <button type="button" aria-current={showCategories ? 'page' : undefined}
        onClick={() => setSection('categories')}>Categorías</button>
     
          <button type="button" aria-current={section === 'presentations' ? 'page' : undefined}
        onClick={() => setSection('presentations')}>Presentaciones</button>
     
    </nav>}
    {/* Productos reúne el listado y la creación en una sola pantalla. */}
    {canCreate && section === 'products' ? <CreateProductPage /> : canCreate && section === 'brands' ? <BrandsPage /> : showCategories ? <CreateCategoryPage /> : <PresentationsPage />}
  </div>
}


