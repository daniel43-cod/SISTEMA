import { LoginForm } from '../components/LoginForm'
import { InstallAppButton } from '../components/InstallAppButton'
import './StaffLoginPage.css'

//pagina principal
export function StaffLoginPage() {
  return (
    <main className="login-page">
      <header className="login-intro">
        <div className="brand">
          <h1>Distribuidora San Antonio<span className="brand-subtitle">Sistema</span></h1>
          <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} alt="" width="48" height="48" />
        </div>
      </header>
      <section className="login-panel" aria-labelledby="login-title">
        <div className="login-card">
          <h2 id="login-title">Bienvenido</h2>
          <p className="login-description">Ingresa con tu cuenta de empleado o administrador.</p>
          <LoginForm />
          <InstallAppButton />
          <div className="login-help">
            <strong>¿Necesitas acceso?</strong>
            <p>Solicita tu cuenta o ayuda para ingresar al administrador del negocio.</p>
          </div>
        </div>
      </section>
    </main>
  )
}
