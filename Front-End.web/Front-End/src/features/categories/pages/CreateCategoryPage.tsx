import { useAuth } from '../../auth'
import { CategoryForm } from '../components/CategoryForm'
import './CreateCategoryPage.css'

export function CreateCategoryPage() {
  const { session } = useAuth()
  if (!session) return null
  if (session.user.role !== 'ADMINISTRADOR')
    return <p>Solo un administrador puede crear categorías.</p>

  return <section className="category-card" aria-labelledby="create-category-title">
    <h2 id="create-category-title">Crear categoría</h2>
    <p className="category-description">Organiza los productos de tu negocio por categoría.</p>
    <CategoryForm />
  </section>
}
