import { useId, useState } from 'react'
import type { SaleDetail } from '../api/salesApi'
import './SaleDetailSurtido.css'

export function SaleDetailSurtido({ detail }: { detail: SaleDetail }) {
  const [open, setOpen] = useState(false)
  const id = useId()
  return <>
    <div className="sale-detail-surtido-heading">
      <span>{[detail.descripcion_resentacion, detail.nombre_producto].filter(Boolean).join(' · ')}</span>
      <button type="button" aria-label={`Ver u ocultar composición de ${detail.nombre_producto}`} aria-expanded={open} aria-controls={id}
        onClick={() => setOpen(value => !value)} title="Detalles del surtido">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" aria-hidden="true"><path d="M4 6h16M4 12h16M4 18h16" /></svg>
      </button>
    </div>
    {open && <ul id={id} className="sale-detail-surtido-components">{detail.componentes?.map(c => <li key={c.id_producto}>
      <span>{c.nombre}</span><span>{c.cantidad} {c.cantidad === 1 ? 'unidad' : 'unidades'}</span>
    </li>)}</ul>}
  </>
}
