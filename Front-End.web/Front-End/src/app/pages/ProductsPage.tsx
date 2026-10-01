import { useState } from 'react'
import { useAuth } from '../../features/auth'
import { PresentationsPage } from '../../features/presentations'
import { CreateCategoryPage } from '../../features/categories'
import './ProductsPage.css'

export function ProductsPage() {
  const { session } = useAuth()
  const [section, setSection] = useState<'presentations' | 'categories'>('presentations')
  if (!session) return null
  const canCreate = session.user.role === 'ADMINISTRADOR'
  const showCategories = canCreate && section === 'categories'

  return <div>
    {canCreate && <nav className="products-sections" aria-label="Catálogos de productos">
      <button type="button" aria-current={!showCategories ? 'page' : undefined}
        onClick={() => setSection('presentations')}>Presentaciones</button>
      <button type="button" aria-current={showCategories ? 'page' : undefined}
        onClick={() => setSection('categories')}>Categorías</button>
    </nav>}
    {showCategories ? <CreateCategoryPage /> : <PresentationsPage />}
  </div>
}
