import { LoginForm } from '../components/LoginForm'
import './StaffLoginPage.css'

export function StaffLoginPage() {
  return (
    <main className="login-page">
      <section className="login-intro" aria-labelledby="intro-title">
        <div className="brand">
          <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} alt="" width="48" height="48" />
          <span>Sistema<span className="brand-subtitle">PUNTO DE VENTA</span></span>
        </div>
        <div className="intro-content">
          <span className="eyebrow">TU NEGOCIO, EN UN SOLO LUGAR</span>
          <h1 id="intro-title">Todo listo para<br />un nuevo día.</h1>
          <p>Ventas, inventario y caja. Las herramientas de tu equipo, en un mismo espacio.</p>
         
        </div>
        <p className="intro-footer">Más orden para trabajar. Más tiempo para tus clientes.</p>
      </section>
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
        <p className="panel-footer">Espacio de trabajo para empleados y administradores.</p>
      </section>
    </main>
  )
}