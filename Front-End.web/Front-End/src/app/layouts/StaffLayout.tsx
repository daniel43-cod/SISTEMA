import { useEffect, useId, useRef, useState } from 'react'
import { useAuth } from '../../features/auth'
import { PresentationsPage } from '../../features/presentations'
import { StaffNavigation } from '../navigation/StaffNavigation'
import { getStaffNavigation } from '../navigation/navigationItems'
import type { StaffSection } from '../navigation/navigationItems'
import { NavigationIcon } from '../navigation/NavigationIcon'
import './StaffLayout.css'

export function StaffLayout() {
  const { session, logout } = useAuth()
  const [selected, setSelected] = useState<StaffSection>('inicio')
  const [menuOpen, setMenuOpen] = useState(false)
  const contentRef = useRef<HTMLElement>(null)
  const [accountOpen, setAccountOpen] = useState(false)
  const accountRef = useRef<HTMLDivElement>(null)
  const accountButtonRef = useRef<HTMLButtonElement>(null)
  const accountPanelId = useId()

  useEffect(() => {
    if (!accountOpen) return
    const closeOutside = (event: PointerEvent) => {
      if (event.target instanceof Node && !accountRef.current?.contains(event.target)) {
        setAccountOpen(false)
      }
    }
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setAccountOpen(false)
        accountButtonRef.current?.focus()
      }
    }
    document.addEventListener('pointerdown', closeOutside)
    document.addEventListener('keydown', closeOnEscape)
    return () => {
      document.removeEventListener('pointerdown', closeOutside)
      document.removeEventListener('keydown', closeOnEscape)
    }
  }, [accountOpen])

  if (!session) return null

  const items = getStaffNavigation(session.user.role)
  // La navegación solo muestra secciones del rol actual; la API valida permisos reales.
  const current = items.find(item => item.id === selected) ?? items[0]
  const roleLabel = session.user.role === 'ADMINISTRADOR' ? 'Administrador' : 'Vendedor'
  function selectSection(section: StaffSection) {
    setSelected(section)
    setMenuOpen(false)
    contentRef.current?.focus()
  }

  return (
    <div className="staff-shell">
      <a className="staff-skip" href="#staff-content">Ir al contenido</a>
      <aside className="staff-sidebar">
        <div className="staff-brand">
          <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} width="42" height="42" alt="" />
          <div><strong>Distribuidora San Antonio</strong><span>Punto de venta</span></div>
        </div>
    
        <div id="staff-menu" className={`staff-menu ${menuOpen ? 'is-open' : ''}`}
          onKeyDown={event => {
            if (event.key === 'Escape') {
              setMenuOpen(false)
              document.querySelector<HTMLButtonElement>('.staff-menu-toggle')?.focus()
            }
          }}>
          <StaffNavigation items={items} selected={current.id} onSelect={selectSection} />
          <div className="staff-sidebar-footer"><span>Sesión de trabajo</span><strong>{roleLabel}</strong></div>
        </div>
      </aside>

      <div className="staff-workspace">
        <header className="staff-header">
          <button type="button" className="staff-menu-toggle"
            aria-label={menuOpen ? 'Cerrar menú' : 'Abrir menú'} aria-expanded={menuOpen}
            aria-controls="staff-menu" onClick={() => {
              setMenuOpen(open => !open)
              setAccountOpen(false)
            }}>
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none"
              stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true">
              <path d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
          <div className="staff-header-brand">
            <img src={`${import.meta.env.BASE_URL}icono_principal.jpeg`} width="34" height="34" alt="" />
            <strong>Distribuidora San Antonio</strong>
          </div>
          <div className="staff-header-section"><span className="staff-breadcrumb">Buenos días</span><strong>{current.label}</strong></div>
          <div className="staff-account" ref={accountRef}
            onBlur={event => {
              if (!event.currentTarget.contains(event.relatedTarget)) setAccountOpen(false)
            }}>
            <button type="button" className="staff-account-toggle" ref={accountButtonRef}
              aria-label="Opciones de usuario" title="Opciones de usuario"
              aria-expanded={accountOpen} aria-controls={accountPanelId}
              onClick={() => { setAccountOpen(open => !open); setMenuOpen(false) }}>
              <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <circle cx="12" cy="8" r="4" />
                <path d="M4 21v-2a8 8 0 0 1 16 0v2" />
              </svg>
            </button>
            <div id={accountPanelId} className="staff-account-panel" hidden={!accountOpen}>
              <div className="staff-user"><strong>{session.user.name}</strong><span>{roleLabel}</span></div>
              <button type="button" className="staff-logout" onClick={() => logout()}>
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor"
                  strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M9 4H4v16h5M10 12h11m-4-4 4 4-4 4" />
                </svg>
                Cerrar sesión
              </button>
            </div>
          </div>
        </header>
        <main id="staff-content" className="staff-content" ref={contentRef} tabIndex={-1}>
          <div className="staff-page-heading">
            <div>{current.id !== 'inicio' && <span className="staff-eyebrow">{current.group}</span>}
              <h1>{current.id === 'inicio' ? `Bienvenido, ${session.user.name}` : current.label}</h1>
              {current.id !== 'inicio' && <p>{current.description}</p>}
            </div>
          </div>
          {current.id === 'inicio' ? <section aria-labelledby="staff-shortcuts-title">
            <h2 id="staff-shortcuts-title" className="staff-section-title">Accesos rápidos</h2>
            <div className="staff-shortcuts">
              {items.filter(item => ['ventas', 'caja', 'reportes'].includes(item.id)).map(item =>
                <button type="button" className="staff-shortcut" key={item.id} onClick={() => selectSection(item.id)}>
                  <span className="staff-shortcut-icon"><NavigationIcon section={item.id} /></span>
                  <strong>{item.label}</strong><span>{item.description}</span>
                  <span className="staff-shortcut-action">Abrir sección <span aria-hidden="true">→</span></span>
                </button>)}
            </div>
          </section> : current.id === 'productos' ? <PresentationsPage /> : <section className="staff-empty" aria-labelledby="staff-empty-title">
            <span className="staff-empty-icon"><NavigationIcon section={current.id} /></span>
            <h2 id="staff-empty-title">{current.label} estará disponible próximamente</h2>
            <p>Por ahora, las operaciones de esta sección aún no están habilitadas.</p>
            <button type="button" className="staff-text-link" onClick={() => selectSection('inicio')}>Volver al inicio</button>
          </section>}
        </main>
      </div>
    </div>
  )
}