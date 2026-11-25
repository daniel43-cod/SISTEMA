import { useId, useState } from 'react'
import type { ReactNode } from 'react'
import type { SaleLine } from '../utils/saleValidation'
import './SaleSurtidoBreakdown.css'

export function SaleSurtidoBreakdown({ line, children }: { line?: SaleLine | null; children: ReactNode }) {
  const [open, setOpen] = useState(false)
  const id = useId()
  if (!line?.product.id_surtido) return <>{children}</>
  return <div className="sale-surtido-breakdown">
    <div className="sale-surtido-inline">{children}
      <button type="button" className="sale-surtido-toggle" aria-expanded={open} aria-controls={id}
        aria-label={`Ver u ocultar productos de ${line.product.nombre}`} title="Ver detalles" onClick={() => setOpen(value => !value)}>
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m6 9 6 6 6-6" /></svg>
      </button>
    </div>
    {open && <ul id={id} aria-label="Composición del surtido">{line.componentes?.map(c => <li key={c.id_producto}>
      <span>{line.product.componentes?.find(p => p.id_producto === c.id_producto)?.nombre ?? 'Producto'}</span>
      <span>{c.cantidad} {c.cantidad === 1 ? 'unidad' : 'unidades'}</span>
    </li>)}</ul>}
  </div>
}
