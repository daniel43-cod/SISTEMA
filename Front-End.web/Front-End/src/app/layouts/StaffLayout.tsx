//contenido de la pagina que aparece cuando el cliente ya se autentico
import { useAuth } from '../../features/auth'
import { Button } from '../../shared/ui/Button'
import './StaffLayout.css'

export function StaffLayout() {
  const { session, logout } = useAuth()
  if (!session) return null
  return (
    <main className="staff-layout">
      <header className="staff-header">
        <strong>Distribuidora san antonio</strong>
        <Button onClick={() => logout()}>Cerrar sesión</Button>
      </header>
      <section className="staff-welcome">
        <span className="access-label">{session.user.role === 'ADMINISTRADOR' ? 'ADMINISTRACIÓN' : 'EQUIPO DE VENTAS'}</span>
        <h1>Bienvenido, {session.user.name}</h1>
        <p>Has iniciado sesión correctamente.</p>
      </section>
    </main>
  )
}