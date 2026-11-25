import { useEffect, useId, useRef } from 'react'
import type { FormEvent } from 'react'
import { createPortal } from 'react-dom'
import { cashCents } from '../utils/saleValidation'
import './SaleCheckoutDialog.css'

const money = (cents: number) => new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' }).format(cents / 100)
export function SaleCheckoutDialog({ total, received, onReceived, pending, blocked, error, onRegister, onClose }: {
  total: number; received: string; onReceived: (value: string) => void; pending: boolean; blocked: boolean; error: string
  onRegister: (event: FormEvent<HTMLFormElement>) => void; onClose: () => void
}) {
  const dialog = useRef<HTMLDialogElement>(null)
  const input = useRef<HTMLInputElement>(null)
  const title = useId()
  const payment = received.trim() ? cashCents(received) : 0
  useEffect(() => {
    const element = dialog.current
    const trigger = document.activeElement instanceof HTMLElement ? document.activeElement : null
    const viewport = window.visualViewport
    function position() {
      const height = viewport?.height ?? window.innerHeight
      element?.style.setProperty('--checkout-height', `${Math.max(1, height - 24)}px`)
      element?.style.setProperty('--checkout-top', `${(viewport?.offsetTop ?? 0) + Math.max(12, (height - (element?.getBoundingClientRect().height ?? 0)) / 2)}px`)
    }
    position(); element?.showModal(); position(); input.current?.focus({ preventScroll: true }); input.current?.select()
    const observer = new ResizeObserver(position); if (element) observer.observe(element)
    viewport?.addEventListener('resize', position); viewport?.addEventListener('scroll', position)
    const overflow = document.body.style.overflow; document.body.style.overflow = 'hidden'
    return () => {
      observer.disconnect(); viewport?.removeEventListener('resize', position); viewport?.removeEventListener('scroll', position)
      element?.close(); document.body.style.overflow = overflow; trigger?.focus()
    }
  }, [])
  return createPortal(<dialog ref={dialog} className="sales-payment-dialog sales-checkout-dialog" aria-labelledby={title}
    onCancel={event => { event.preventDefault(); if (!pending) onClose() }}>
    <form onSubmit={onRegister} aria-busy={pending}>
      <h2 id={title}>Cobrar venta</h2>
      <p className="sales-checkout-dialog-total">Total <strong>{money(total)}</strong></p>
      <label>Pago inicial (Q)<input ref={input} type="text" inputMode="decimal" placeholder="0.00" value={received}
        disabled={blocked} aria-invalid={payment === null} onChange={event => onReceived(event.target.value)} /></label>
      <p>Saldo pendiente <strong>{payment === null ? '—' : money(Math.max(0, total - payment))}</strong></p>
      {payment !== null && payment > total && <p>Cambio <strong>{money(payment - total)}</strong></p>}
      {payment === null && <p role="alert" className="sales-error">Ingresa un importe válido con hasta dos decimales.</p>}
      {error && <p role="alert" className="sales-error">{error}</p>}
      <div className="sales-payment-dialog-actions"><button type="button" disabled={pending} onClick={onClose}>Cancelar</button>
        <button type="submit" className="sales-primary" disabled={blocked || payment === null}>{pending ? 'Registrando…' : 'Registrar venta'}</button></div>
    </form>
  </dialog>, document.body)
}
