import { CustomerChat } from '../../customer-chat/components/CustomerChat'
import { CustomerLoginForm } from '../../customer-auth/components/CustomerLoginForm'
import { useCustomerSession } from '../../customer-auth/hooks/useCustomerSession'
import { useEffect, useId, useRef } from 'react'

export function FloatingPublicAssistant({ open, onOpenChange }: {
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const { session, login, logout, notice } = useCustomerSession()
  const buttonRef = useRef<HTMLButtonElement>(null)
  const closeRef = useRef<HTMLButtonElement>(null)
  const panelId = useId()
  const titleId = useId()
  useEffect(() => { if (open) closeRef.current?.focus() }, [open])
  function close() {
    onOpenChange(false)
    buttonRef.current?.focus()
  }
  return <>
    <button ref={buttonRef} type="button" className="public-assistant-floating" title="Consultar al asistente"
      aria-label="Abrir asistente virtual" aria-expanded={open}
      aria-controls={panelId} hidden={open} onClick={() => onOpenChange(true)}>
      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M12 3v3" /><circle cx="12" cy="2" r="1" />
        <rect x="4" y="6" width="16" height="13" rx="4" />
        <path d="M2 11v4m20-4v4M8 19v3l4-3" />
        <circle cx="9" cy="11" r="1" /><circle cx="15" cy="11" r="1" />
        <path d="M9 15h6" />
      </svg>
    </button>
    <aside id={panelId} className="public-assistant-sidebar" hidden={!open} aria-labelledby={titleId}
      onKeyDown={event => {
        if (event.key === 'Escape') { event.stopPropagation(); close() }
      }}>
      <header className="public-assistant-sidebar-header">
        <h2 id={titleId}>Asistente virtual</h2>
        <button ref={closeRef} type="button" className="public-assistant-close" aria-label="Cerrar asistente"
          onClick={close}>×</button>
      </header>
      <div className="public-assistant-sidebar-body">
        {session ? <>

          {notice && <p role="status">{notice}</p>}
          <CustomerChat key={session.idCuentaCliente} token={session.token} onUnauthorized={logout} email={session.correoElectronico} />
        </> : <div className="public-assistant-intro">
          <p>Consultá sobre productos, precios y disponibilidad.</p>
          <CustomerLoginForm key={String(open)} onLogin={login} />
        </div>}
      </div>
    </aside>
  </>
}

