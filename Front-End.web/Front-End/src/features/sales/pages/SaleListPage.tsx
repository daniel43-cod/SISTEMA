import { listUsers } from '../../users/api/usersApi'
import { SalePaymentForm } from './SalePaymentForm'
import { useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { loadSaleHistory } from '../api/salesApi'
import type { SaleHistory } from '../api/salesApi'
import './SalesPage.css'
import './SaleListPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })

export function SaleListPage() {
  const { session, logout } = useAuth()
  const token = session?.token
  const isAdmin = session?.user.role === 'ADMINISTRADOR'
  const [userId, setUserId] = useState('')
  const [users, setUsers] = useState<{ id: number; name: string; active: boolean }[]>([])
  const [usersLoading, setUsersLoading] = useState(false)
  const [usersError, setUsersError] = useState('')
  const [selected, setSelected] = useState<SaleHistory | null>(null)
  const [saving, setSaving] = useState(false)
  const [success, setSuccess] = useState('')
  const [page, setPage] = useState(1)
  const [revision, setRevision] = useState(0)
  const [rows, setRows] = useState<SaleHistory[]>([])
  const [hasNext, setHasNext] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!token || !isAdmin) return
    const controller = new AbortController()
    setUsersLoading(true); setUsersError('')
    void listUsers(token, controller.signal).then(result => {
      if (!controller.signal.aborted) setUsers(result.map(user => ({ id: user.id_usuario,
        name: (user.nombre + ' ' + user.apellido).trim() + ' (' + user.usuario + ')', active: user.estado })))
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setUsersError(failure instanceof Error ? failure.message : 'No se pudieron cargar los usuarios.')
    }).finally(() => { if (!controller.signal.aborted) setUsersLoading(false) })
    return () => controller.abort()
  }, [token, isAdmin, revision, logout])

  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    setLoading(true); setError(''); setRows([]); setHasNext(false)
    async function load() {
      try {
        const result = await loadSaleHistory(token!, controller.signal, page, isAdmin && userId ? Number(userId) : undefined)
        // La API devuelve un arreglo sin total; consultar la siguiente página evita
        // habilitar Siguiente cuando la última página tiene exactamente 50 ventas.
        const next = result.length === 50 ? await loadSaleHistory(token!, controller.signal, page + 1, isAdmin && userId ? Number(userId) : undefined) : []
        if (controller.signal.aborted) return
        if (result.length === 0 && page > 1) { setPage(1); return }
        setRows(result); setHasNext(next.length > 0)
      } catch (failure) {
        if (controller.signal.aborted) return
        if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
        else setError(failure instanceof Error ? failure.message : 'No se pudieron cargar las ventas.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, page, revision, logout, userId, isAdmin])

  return <section className="sales-page sale-list-page" aria-label="Ventas de la caja abierta">
    {success && <p role="status">{success}</p>}
    {selected && <SalePaymentForm key={selected.id_ventas} sale={selected} onSaving={setSaving}
      onClose={() => { setSelected(null); setRevision(v => v + 1) }}
      onPaid={() => { setSuccess('Abono registrado correctamente.'); setSelected(null); setRevision(v => v + 1) }} />}
    <div className="sales-card">
      <div className="sales-heading">
      {isAdmin && <div className="sale-list-filters">
        <label> Ver por usuario
          <select value={userId} disabled={saving || usersLoading} onChange={event => { setUserId(event.target.value); setPage(1); setSelected(null); setSuccess('') }}>
            <option value="">Todos</option>
            {users.map(user => <option key={user.id} value={user.id}>{user.name}{user.active ? '' : ' · Inactivo'}</option>)}
          </select>
        </label>
        {usersLoading && <span role="status">Cargando usuarios…</span>}
        {usersError && <div role="alert"><p>{usersError}</p><button type="button" disabled={saving} onClick={() => setRevision(v => v + 1)}>Reintentar usuarios</button></div>}
      </div>}
        <RefreshButton label="Actualizar ventas" loading={loading || saving} onClick={() => { setPage(1); setRevision(v => v + 1) }} />
      </div>

      {error && <div role="alert" className="sale-list-error"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div>}
      <div aria-busy={loading}>
        {loading ? <p role="status">Cargando ventas…</p> : !error && rows.length === 0 ?
          <p role="status">{userId ? 'Este usuario no tiene ventas en la caja activa, o la caja está cerrada.' : 'No hay ventas para mostrar. La caja puede estar cerrada o aún no tener ventas.'}</p> : !error && <>
            <div className="sales-table-scroll" role="region" aria-label="Listado de ventas" tabIndex={0}>
              <table><caption className="sale-list-caption">Ventas registradas en la sesión de caja abierta</caption>
                <thead><tr><th scope="col">Fecha y hora</th><th scope="col">Cliente</th><th scope="col">Responsable</th><th scope="col" className="sale-list-money">Total</th>{isAdmin && <th scope="col" className="sale-list-money">Ganancia</th>}<th scope="col" className="sale-list-money">Saldo pendiente</th><th scope="col">Estado</th></tr></thead>
                <tbody>{rows.map(sale => <tr key={sale.id_ventas} className={selected?.id_ventas === sale.id_ventas ? 'sale-list-row is-selected' : 'sale-list-row'}
                  tabIndex={saving ? -1 : 0} aria-label={'Ver detalle de la venta de ' + sale.cliente} aria-expanded={selected?.id_ventas === sale.id_ventas}
                  onKeyDown={event => { if (!saving && (event.key === 'Enter' || event.key === ' ')) { event.preventDefault(); setSelected(sale); setSuccess('') } }}
                  onClick={() => { if (!saving) { setSelected(sale); setSuccess('') } }}>
                  <td className="sale-list-nowrap">{Number.isNaN(Date.parse(sale.fecha_venta)) ? 'Fecha no disponible' : date.format(new Date(sale.fecha_venta))}</td>
                  <td>{sale.cliente}</td><td>{sale.usuario}</td><td className="sale-list-money">{money.format(sale.total)}</td>
                  {isAdmin && <td className="sale-list-money">{sale.ganancia_total === undefined ? 'No disponible' : money.format(sale.ganancia_total)}</td>}
                  <td className="sale-list-money">{money.format(sale.saldo_pendiente)}</td>
                  <td><span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.estado}</span></td>
                </tr>)}</tbody>
              </table>
            </div>
          </>}
      </div>
      <nav className="sales-history-pagination" aria-label="Páginas de ventas">
        <button type="button" disabled={loading || saving || page === 1} onClick={() => setPage(v => v - 1)}>Anterior</button>
        <span role="status">Página {page}</span>
        <button type="button" disabled={loading || saving || !!error || !hasNext} onClick={() => setPage(v => v + 1)}>Siguiente</button>
      </nav>
    </div>
  </section>
}
