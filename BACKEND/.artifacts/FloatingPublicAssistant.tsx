import { useEffect, useId, useRef, useState } from 'react'

export function FloatingPublicAssistant() {
  const [open, setOpen] = useState(false)
  const dialogRef = useRef<HTMLDialogElement>(null)
  const dialogId = useId()
  const titleId = useId()
  useEffect(() => {
    if (!open || !dialogRef.current) return
    const dialog = dialogRef.current
    dialog.showModal()
    const previous = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => { dialog.close(); document.body.style.overflow = previous }
  }, [open])
  return <>
    <button type="button" className="public-assistant-floating" title="Consultar al asistente"
      aria-label="Abrir asistente virtual" aria-haspopup="dialog" aria-expanded={open}
      aria-controls={dialogId} onClick={() => setOpen(true)}>
      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M12 3v3" /><circle cx="12" cy="2" r="1" />
        <rect x="4" y="6" width="16" height="13" rx="4" />
        <path d="M2 11v4m20-4v4M8 19v3l4-3" />
        <circle cx="9" cy="11" r="1" /><circle cx="15" cy="11" r="1" />
        <path d="M9 15h6" />
      </svg>
    </button>
    <dialog ref={dialogRef} id={dialogId} className="public-assistant-dialog"
      aria-labelledby={titleId} onCancel={() => setOpen(false)} onClose={() => setOpen(false)}
      onClick={event => { if (event.target === event.currentTarget) setOpen(false) }}>
      <section className="public-assistant-intro">
        <button type="button" className="public-cart-close" aria-label="Cerrar asistente"
          onClick={() => setOpen(false)} autoFocus>×</button>
        <h2 id={titleId}>Asistente virtual</h2>
        <p>Consultá sobre productos, precios y disponibilidad.</p>
        <p>Para conversar con el asistente necesitarás iniciar sesión o crear una cuenta de cliente.</p>
      </section>
    </dialog>
  </>
}

