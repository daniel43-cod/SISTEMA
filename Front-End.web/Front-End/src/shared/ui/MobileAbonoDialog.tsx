import { useEffect, useId, useRef, useState } from 'react'
import type { ReactNode } from 'react'
import { createPortal } from 'react-dom'
import './MobileAbonoDialog.css'

export function MobileAbonoDialog({ active, title, busy, onClose, className = '', children }: {
  active: boolean; title: string; busy: boolean; onClose: () => void; className?: string; children: ReactNode
}) {
  const panel = useRef<HTMLDialogElement>(null)
  const headingId = useId()
  const [mobile, setMobile] = useState(() => window.matchMedia('(max-width: 800px)').matches)
  useEffect(() => {
    const media = window.matchMedia('(max-width: 800px)')
    const update = () => setMobile(media.matches)
    media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [])
  useEffect(() => {
    if (!active || !mobile) return
    const dialog = panel.current
    if (!dialog) return
    const previous = document.activeElement
    const viewport = window.visualViewport
    const position = () => {
      const height = viewport?.height ?? window.innerHeight
      dialog.style.top = `${(viewport?.offsetTop ?? 0) + height / 2}px`
      dialog.style.maxHeight = `${Math.max(100, height - 24)}px`
    }
    position(); dialog.showModal()
    dialog.querySelector<HTMLInputElement>('input:not(:disabled):not([readonly])')?.focus()
    viewport?.addEventListener('resize', position)
    viewport?.addEventListener('scroll', position)
    window.addEventListener('resize', position)
    return () => {
      viewport?.removeEventListener('resize', position)
      viewport?.removeEventListener('scroll', position)
      window.removeEventListener('resize', position)
      dialog.close()
      if (previous instanceof HTMLElement && previous.isConnected) previous.focus()
    }
  }, [active, mobile])
  if (!active || !mobile) return <>{children}</>
  return createPortal(<dialog ref={panel} className={`mobile-abono-overlay ${className}`} aria-labelledby={headingId}
    onCancel={event => { event.preventDefault(); if (!busy) onClose() }}>
    <header className="mobile-abono-overlay-heading"><h2 id={headingId}>{title}</h2>
      <button type="button" aria-label="Cerrar formulario de abono" disabled={busy} onClick={onClose}>×</button>
    </header>
    {children}
  </dialog>, document.body)
}
