import { LoginForm } from '../components/LoginForm'
import './StaffLoginPage.css'

//pagina principal
export function StaffLoginPage() {
  return (
    <main className="login-page">
      <header className="login-intro">
        <div className="brand">
          <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} alt="" width="48" height="48" />
          <h1>Sistema<span className="brand-subtitle">PUNTO DE VENTA</span></h1>
        </div>
      </header>
      <section className="login-panel" aria-labelledby="login-title">
        <div className="login-card">
          <span className="access-label">ACCESO DEL EQUIPO</span>
          <h2 id="login-title">Bienvenido de nuevo</h2>
          <p className="login-description">Ingresa con tu cuenta de empleado o administrador.</p>
          <LoginForm />
          <div className="login-help">
            <strong>¿Necesitas acceso?</strong>
            <p>Solicita tu cuenta o ayuda para ingresar al administrador del negocio.</p>
          </div>
        </div>
      </section>
    </main>
  )
}