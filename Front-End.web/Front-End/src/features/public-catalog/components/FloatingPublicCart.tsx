import { useEffect, useId, useRef, useState } from 'react'
import type { ComponentProps } from 'react'
import { PublicCart } from './PublicCart'

export function FloatingPublicCart(props: ComponentProps<typeof PublicCart>) {
  const [open, setOpen] = useState(false)
  const dialogRef = useRef<HTMLDialogElement>(null)
  const dialogId = useId()
  useEffect(() => {
    const dialog = dialogRef.current
    if (!dialog || !open) return
    dialog.showModal()
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => {
      dialog.close()
      document.body.style.overflow = previousOverflow
    }
  }, [open])
  return <>
    <button type="button" className="public-cart-floating"
      aria-label={`Abrir carrito: ${props.count} artículos, total estimado ${props.total.toFixed(2)} quetzales`}
      aria-haspopup="dialog" aria-controls={dialogId} aria-expanded={open}
      onClick={() => setOpen(true)}>
      <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor"
        strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M3 3h2l2.4 12h11.2L21 7H6" />
        <circle cx="9" cy="20" r="1" /><circle cx="18" cy="20" r="1" />
      </svg>
      <span className="public-cart-badge" aria-hidden="true">{props.count > 99 ? '99+' : props.count}</span>
      <span className="public-cart-floating-total" aria-hidden="true">
        <small>Total estimado</small>
        <strong>{new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(props.total)}</strong>
      </span>
    </button>
    <span className="public-cart-announcement" role="status" aria-live="polite" aria-atomic="true">
      {props.count} artículos en el carrito. Total estimado: {props.total.toFixed(2)} quetzales.
    </span>
    <dialog ref={dialogRef} id={dialogId} className="public-cart-dialog"
      aria-labelledby="public-cart-title" onCancel={() => setOpen(false)} onClose={() => setOpen(false)}
      onClick={event => { if (event.target === event.currentTarget) setOpen(false) }}>
      <div className="public-cart-dialog-content">
        <button type="button" className="public-cart-close" aria-label="Cerrar carrito"
          onClick={() => setOpen(false)} autoFocus>
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor"
            strokeWidth="2" strokeLinecap="round" aria-hidden="true"><path d="m6 6 12 12M6 18 18 6" /></svg>
        </button>
        <PublicCart {...props} />
      </div>
    </dialog>
  </>
}

