import { LoginForm } from '../components/LoginForm'
import { InstallAppButton } from '../components/InstallAppButton'
import './StaffLoginPage.css'

//pagina principal
export function StaffLoginPage() {
  return (
    <main className="login-page">
      <div className="login-shell">
      <header className="login-intro">
        <div className="brand">
          <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} alt="" width="48" height="48" />
          <span>Distribuidora San Antonio</span>
        </div>
        <div className="login-welcome">
          <p className="login-eyebrow">TU ESPACIO DE TRABAJO</p>
          <h1>Bienvenido<br />a tu sistema.</h1>
          <p>Todo lo que necesitas para gestionar tu negocio, en un solo lugar.</p>
        </div>
        <span className="login-orb login-orb-one" aria-hidden="true" />
        <span className="login-orb login-orb-two" aria-hidden="true" />
      </header>
      <section className="login-panel" aria-labelledby="login-title">
        <div className="login-card">
          <h2 id="login-title">Iniciar sesión</h2>
          <p className="login-description">Ingresa con tu cuenta.</p>
          <LoginForm />
          <InstallAppButton />
         
        </div>
      </section>
      </div>
    </main>
  )
}
