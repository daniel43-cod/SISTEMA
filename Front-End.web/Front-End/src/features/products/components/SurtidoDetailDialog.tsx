import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { useAuth } from '../../auth'
import { getSurtido } from '../api/surtidosApi'
import type { SurtidoDetail } from '../api/surtidosApi'
import { ApiError } from '../../../shared/api/ApiError'
import { ProductDetailImage } from './ProductDetailImage'
import { SurtidoForm } from './SurtidoForm'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
export function SurtidoDetailDialog({ id, token, onClose, startEditing = false, onUpdated }: {
  id: number; token: string; onClose: () => void; startEditing?: boolean; onUpdated?: () => void
}) {
  const { logout } = useAuth()
  const dialog = useRef<HTMLDialogElement>(null)
  const [data, setData] = useState<SurtidoDetail | null>(null)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [editing, setEditing] = useState(startEditing)
  const [pending, setPending] = useState(false)
  useEffect(() => {
    const element = dialog.current
    const trigger = document.activeElement instanceof HTMLElement ? document.activeElement : null
    element?.showModal()
    const overflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    return () => { element?.close(); document.body.style.overflow = overflow; trigger?.focus() }
  }, [])
  useEffect(() => {
    const controller = new AbortController()
    void getSurtido(id, token, controller.signal).then(result => {
      if (!controller.signal.aborted) setData(result)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof ApiError ? failure.message : 'No se pudo cargar el surtido.')
    })
    return () => controller.abort()
  }, [id, token, revision, logout])
  return createPortal(<dialog ref={dialog} className="product-card product-detail-dialog" aria-labelledby="surtido-detail-title"
    onCancel={event => { event.preventDefault(); if (!pending) onClose() }}>
    <div className="product-detail-header"><h2 id="surtido-detail-title">{editing ? 'Editar surtido' : 'Detalles del surtido'}</h2>
      <div className="product-detail-header-actions">{data && !editing && <button type="button" className="Product-update-button" onClick={() => setEditing(true)}>Editar</button>}
      <button type="button" className="product-detail-close" aria-label="Cerrar detalles" disabled={pending} onClick={onClose}>×</button></div></div>
    {!data && !error && <p role="status">Cargando detalles…</p>}
    {error && <><p role="alert" className="product-error">{error}</p><button type="button" onClick={() => { setError(''); setRevision(v => v + 1) }}>Reintentar</button></>}
    {data && editing && <SurtidoForm initial={data} onPending={setPending} onCancel={() => setEditing(false)} onSaved={() => {
      setEditing(false); setData(null); setRevision(v => v + 1); onUpdated?.()
    }} />}
    {data && !editing && <>
      <div className="product-detail-overview"><div className="product-detail-info"><h3>{data.nombre}</h3>
        <dl className="product-detail-fields"><div><dt>Marca</dt><dd>{data.marca}</dd></div>
          <div><dt>Estado</dt><dd>{data.estado ? 'Activo' : 'Inactivo'}</dd></div></dl></div>
        <ProductDetailImage image={data.imagen} name={data.nombre} /></div>
      <h3>Presentaciones</h3>
      <div className="product-presentations-table-scroll"><table className="product-presentations-table"><thead><tr>
        <th>Presentación</th><th>Unidades</th><th>Precio</th><th>Precio mínimo actual</th></tr></thead><tbody>
        {data.presentaciones.map(p => <tr key={p.idProductoPresentacion}><td>{p.presentacion}</td><td>{p.unidadesEquivalentes}</td>
          <td>{money.format(p.precio)}</td><td>{p.precioMinimoActual === null ? 'Costos incompatibles' : money.format(p.precioMinimoActual)}</td></tr>)}
      </tbody></table></div>
      <h3>Productos permitidos</h3><ul>{data.productos.map(p => <li key={p.idProducto}>{p.nombre}</li>)}</ul>
    </>}
  </dialog>, document.body)
}
