import { useAuth } from '../../auth'
import { PresentationForm } from '../components/PresentationForm'
import { PresentationList } from '../components/PresentationList'
import { usePresentations } from '../hooks/usePresentations'
import './PresentationsPage.css'

export function PresentationsPage() {
  const { session } = useAuth()
  const { items, loading, error, reload } = usePresentations()
  if (!session) return null
  return <div className="presentations-page">
    {session.user.role === 'ADMINISTRADOR' && <section className="presentation-card" aria-labelledby="create-presentation-title">
      <span className="presentation-label">Presentaciones</span>
      <h2 id="create-presentation-title">Crear presentación</h2>
      <p className="presentation-description">Registra los formatos que utilizarás al crear tus productos.</p>
      <PresentationForm onCreated={reload} />
    </section>}
    <PresentationList items={items} loading={loading} error={error} onReload={reload} />
  </div>
}