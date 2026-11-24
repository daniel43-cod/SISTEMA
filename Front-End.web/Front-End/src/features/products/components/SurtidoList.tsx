import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { listSurtidos } from '../api/surtidosApi'
import type { SurtidoPage } from '../api/surtidosApi'
import './SurtidoList.css'
import { SurtidoDetailDialog } from './SurtidoDetailDialog'

export function SurtidoList({ actions, idMarca, embedded = false }: { actions?: ReactNode; idMarca?: number; embedded?: boolean }) {
  const { session, logout } = useAuth()
  const token = session?.user.role === 'ADMINISTRADOR' ? session.token : undefined
  const [text, setText] = useState('')
  const [selected, setSelected] = useState<number | null>(null)
  const [query, setQuery] = useState({ text: '', page: 1, revision: 0 })
  const [result, setResult] = useState<{ token: string; query: typeof query; data?: SurtidoPage; error?: string } | null>(null)
  const current = result && result.token === token && result.query === query ? result : null
  const loading = !current
  useEffect(() => {
    const timer = window.setTimeout(() => setQuery(q => q.text === text.trim() ? q : { ...q, text: text.trim(), page: 1 }), 300)
    return () => window.clearTimeout(timer)
  }, [text])
  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    void listSurtidos(query.text, query.page, token, controller.signal, idMarca).then(data => {
      if (!controller.signal.aborted) setResult({ token, query, data })
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setResult({ token, query, error: failure instanceof ApiError ? failure.message : 'No se pudieron cargar los surtidos.' })
    })
    return () => controller.abort()
  }, [token, query, logout, idMarca])
  if (!token) return null
  return <section className={`product-card surtido-list${embedded ? ' surtido-list-embedded' : ''}`} aria-label="Surtidos de la marca" aria-busy={loading}>
    {!embedded && <><div className="surtido-list-heading"><h2>Surtidos</h2><div className="surtido-list-actions">{actions}
      <RefreshButton label="Actualizar surtidos" loading={loading} onClick={() => setQuery(q => ({ ...q, revision: q.revision + 1 }))} /></div></div>
    <label className="field">Buscar surtido
      <input className="input" type="search" value={text} maxLength={200} placeholder="Nombre del surtido" onChange={e => setText(e.target.value)} /></label></>}
    {loading && <p role="status">Cargando surtidos…</p>}
    {current?.error && <p role="alert" className="product-error">{current.error}</p>}
    {current?.data && <>
      {!embedded && <p role="status">{current.data.total} surtidos registrados</p>}
      {!embedded && !current.data.elementos.length && <p>No hay surtidos para mostrar.</p>}
      {!!current.data.elementos.length && <div className="product-table-scroll surtido-rows"><table className="product-table">
        <thead><tr><th>Producto</th><th>Marca</th><th>Estado</th></tr></thead><tbody>
          {current.data.elementos.map(s => <tr key={s.idSurtido} className="surtido-product-row" onClick={() => setSelected(s.idSurtido)}>
            <td data-label="Producto"><button type="button" className="product-name-button" aria-haspopup="dialog" onClick={() => setSelected(s.idSurtido)}>{s.nombre}</button></td>
            <td data-label="Marca">{s.marca}</td><td data-label="Estado"><span className={s.estado ? 'surtido-active' : 'surtido-inactive'}>{s.estado ? 'Activo' : 'Inactivo'}</span></td>
          </tr>)}
        </tbody></table></div>}
      {current.data.total > 20 && <div className="surtido-pagination">
        <button type="button" className="surtido-secondary" disabled={query.page <= 1} onClick={() => setQuery(q => ({ ...q, page: q.page - 1 }))}>Anterior</button>
        <span>Página {query.page} de {Math.ceil(current.data.total / 20)}</span>
        <button type="button" className="surtido-secondary" disabled={query.page * 20 >= current.data.total} onClick={() => setQuery(q => ({ ...q, page: q.page + 1 }))}>Siguiente</button>
      </div>}
    </>}
    {selected !== null && <SurtidoDetailDialog key={selected} id={selected} token={token} onClose={() => setSelected(null)} />}
  </section>
}
