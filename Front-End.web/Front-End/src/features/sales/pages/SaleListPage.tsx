import { listUsers } from '../../users/api/usersApi'
import { lastSevenSaleDays } from '../utils/saleDateRange'
import { SalePaymentForm } from './SalePaymentForm'
import { useEffect, useState } from 'react'
import { createPortal } from 'react-dom'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { loadSaleSummary } from '../api/saleSummaryApi'
import { loadSaleHistory } from '../api/salesApi'
import type { SaleHistory } from '../api/salesApi'
import './SalesPage.css'
import './SaleListPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })

export function SaleListPage({ onAccountTarget, toolbarTarget }: { onAccountTarget?: (node: HTMLDivElement | null) => void; toolbarTarget?: HTMLDivElement | null } = {}) {
  const { session, logout } = useAuth()
  const token = session?.token
  const isAdmin = session?.user.role === 'ADMINISTRADOR'
  const dateLimits = lastSevenSaleDays()
  const [range, setRange] = useState<{ desde: string; hasta: string }>()
  const [selectedDate, setSelectedDate] = useState('')
  const [userId, setUserId] = useState('')
  const [users, setUsers] = useState<{ id: number; name: string }[]>([])
  const [usersLoading, setUsersLoading] = useState(false)
  const [usersError, setUsersError] = useState('')
  const [selected, setSelected] = useState<SaleHistory | null>(null)
  const [saving, setSaving] = useState(false)
  const [success, setSuccess] = useState('')
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(50)
  const [revision, setRevision] = useState(0)
  const [rows, setRows] = useState<SaleHistory[]>([])
  const [salesVisible, setSalesVisible] = useState(false)
  const [salesTotal, setSalesTotal] = useState<number | null>(null)
  const [profitTotal, setProfitTotal] = useState<number | null>(null)
  const [profitVisible, setProfitVisible] = useState(false)
  const [hasNext, setHasNext] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [mobileStatus, setMobileStatus] = useState<'all' | 'pending' | 'paid'>('all')
  const [mobileSearchOpen, setMobileSearchOpen] = useState(false)
  const [mobileSearch, setMobileSearch] = useState('')
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false)
  const mobileRows = rows.filter(sale =>
    (mobileStatus === 'all' || (mobileStatus === 'paid' ? sale.saldo_pendiente === 0 : sale.saldo_pendiente > 0)) &&
    (!mobileSearch.trim() || `${sale.cliente} ${sale.id_ventas} ${sale.usuario}`.toLocaleLowerCase().includes(mobileSearch.trim().toLocaleLowerCase())))

  useEffect(() => {
    if (!token || !isAdmin) return
    const controller = new AbortController()
    setUsersLoading(true); setUsersError('')
    void listUsers(token, controller.signal).then(result => {
      if (!controller.signal.aborted) setUsers(result.map(u => ({ id: u.id_usuario, name: (u.nombre + ' ' + u.apellido).trim() + ' (' + u.usuario + ')' + (u.estado ? '' : ' · Inactivo') })))
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
    setLoading(true); setError(''); setRows([]); setHasNext(false); setProfitTotal(null); setSalesTotal(null)
    async function load() {
      try {
        if (isAdmin) {
          const summary = await loadSaleSummary(token!, page, pageSize, range, userId ? Number(userId) : undefined, controller.signal)
          if (controller.signal.aborted) return
          if (summary.items.length === 0 && page > 1) { setPage(1); return }
          setRows(summary.items); setHasNext(summary.hayMas); setProfitTotal(summary.gananciaTotal); setSalesTotal(summary.totalVentas)
          return
        }
        const result = await loadSaleHistory(token!, controller.signal, page, isAdmin ? range : undefined, isAdmin && userId ? Number(userId) : undefined, pageSize)
        // La API devuelve un arreglo sin total; consultar la siguiente página evita
        // habilitar Siguiente cuando la última página tiene exactamente el tamaño de página.
        const next = result.length === pageSize ? await loadSaleHistory(token!, controller.signal, page + 1, isAdmin ? range : undefined, isAdmin && userId ? Number(userId) : undefined, pageSize) : []
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
  }, [token, page, revision, logout, range, isAdmin, userId, pageSize])

  const mobileToolbar = (
<div className="sale-mobile-toolbar">
        <h2>Ventas</h2>
        <button type="button" className="sale-mobile-icon" aria-label="Buscar ventas en esta página" aria-expanded={mobileSearchOpen} onClick={() => setMobileSearchOpen(value => !value)}>
          <svg width="23" height="23" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true"><circle cx="10.5" cy="10.5" r="7" /><path d="m16 16 5 5" /></svg>
        </button>
        {isAdmin && <button type="button" className="sale-mobile-icon sale-mobile-user-filter" aria-label="Filtrar ventas por usuario y fecha" aria-expanded={mobileFiltersOpen} onClick={() => setMobileFiltersOpen(value => !value)}>
          <svg width="23" height="23" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" aria-hidden="true"><path d="M3 5h18M6 11h12M9 17h6" /></svg>
          <span>{Number(!!userId) + Number(!!range)}</span>
        </button>}
      </div>
  )
  return <section className="sales-page sale-list-page sale-list-view" aria-label={isAdmin && range ? 'Historial de ventas por fechas' : 'Ventas de la caja abierta'}>
    {success && <p role="status">{success}</p>}
    {selected && <SalePaymentForm key={selected.id_ventas} sale={selected} onSaving={setSaving}
      onClose={() => { setSelected(null); setRevision(v => v + 1) }}
      onPaid={() => { setSuccess('Abono registrado correctamente.'); setSelected(null); setRevision(v => v + 1) }} />}
    <div className="sales-card">

      <div className="sales-heading">
      {isAdmin && <div className="sale-list-filters sale-date-filters sale-arranged-filters">
<div className="sale-filter-stack"><div className="sale-user-cash-row"><label>Usuario<select value={userId} disabled={saving || usersLoading} onChange={event => {
          setUserId(event.target.value); setPage(1); setSelected(null); setSuccess('')
        }}><option value="">Todos</option>{users.map(u => <option key={u.id} value={u.id}>{u.name}</option>)}</select></label><button type="button" disabled={saving || loading} onClick={() => {
          setSelectedDate(''); setRange(undefined); setUserId(''); setPage(1); setSelected(null); setSuccess('')
        }}>Caja activa</button></div>
<div className="sale-date-row"><label className="sale-filter-date">Fecha<input type="date" value={selectedDate} min={dateLimits.desde} max={dateLimits.hasta} disabled={saving} onChange={event => {
          const day = event.target.value
          if (day && (day < dateLimits.desde || day > dateLimits.hasta)) return
          setSelectedDate(day); setUserId('')
          setRange(day ? { desde: day, hasta: day } : undefined)
          setPage(1); setSelected(null); setSuccess('')
        }} /></label><RefreshButton label="Actualizar ventas" loading={loading || saving} onClick={() => { setPage(1); setRevision(v => v + 1) }} /></div></div>
<div className="sale-totals-stack"><div className="sale-profit-summary"><span>Total de ventas</span><div className="sale-profit-card"><div>
<strong aria-label={salesVisible ? undefined : 'Total de ventas oculto'}>{salesVisible ? (salesTotal === null ? '—' : money.format(salesTotal)) : 'Q ••••••'}</strong>
<button type="button" className="sale-profit-toggle" aria-label={salesVisible ? 'Ocultar total de ventas' : 'Mostrar total de ventas'} title={salesVisible ? 'Ocultar' : 'Mostrar'} aria-pressed={salesVisible} onClick={() => setSalesVisible(v => !v)}>
<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" />{!salesVisible && <path d="m3 3 18 18" />}</svg></button>
</div></div></div><div className="sale-profit-summary"><span>Ganancia total</span><div className="sale-profit-card"><div>
<strong aria-label={profitVisible ? undefined : 'Ganancia oculta'}>{profitVisible ? (profitTotal === null ? '—' : money.format(profitTotal)) : 'Q ••••••'}</strong>
<button type="button" className="sale-profit-toggle" aria-label={profitVisible ? 'Ocultar ganancia total' : 'Mostrar ganancia total'} title={profitVisible ? 'Ocultar' : 'Mostrar'} aria-pressed={profitVisible} onClick={() => setProfitVisible(v => !v)}>
<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" />{!profitVisible && <path d="m3 3 18 18" />}</svg></button>
</div></div></div></div>
{usersLoading && <span role="status">Cargando usuarios…</span>}
        {usersError && <div role="alert"><p>{usersError}</p><button type="button" disabled={saving} onClick={() => setRevision(v => v + 1)}>Reintentar usuarios</button></div>}
      </div>}
        {!isAdmin && <RefreshButton label="Actualizar ventas" loading={loading || saving} onClick={() => { setPage(1); setRevision(v => v + 1) }} />}
      {onAccountTarget && <div className="sale-list-account-row"><div ref={onAccountTarget} /></div>}
      </div>

      {toolbarTarget ? createPortal(mobileToolbar, toolbarTarget) : mobileToolbar}
      {mobileSearchOpen && <label className="sale-mobile-search">Buscar en esta página<input type="search" autoFocus placeholder="Cliente, número de venta o responsable" value={mobileSearch} onChange={event => setMobileSearch(event.target.value)} /></label>}
      {isAdmin && mobileFiltersOpen && <div className="sale-mobile-filters">
        <label>Usuario<select value={userId} disabled={saving || usersLoading} onChange={event => { setUserId(event.target.value); setPage(1); setSelected(null); setSuccess('') }}>
          <option value="">Todos</option>{users.map(user => <option key={user.id} value={user.id}>{user.name}</option>)}
        </select></label>
        <label>Fecha<input type="date" value={selectedDate} min={dateLimits.desde} max={dateLimits.hasta} disabled={saving} onChange={event => {
          const day = event.target.value
          if (day && (day < dateLimits.desde || day > dateLimits.hasta)) return
          setSelectedDate(day); setRange(day ? { desde: day, hasta: day } : undefined); setPage(1); setSelected(null)
        }} /></label>
        <button type="button" disabled={saving || loading} onClick={() => { setSelectedDate(''); setRange(undefined); setUserId(''); setPage(1); setSelected(null) }}>Caja activa</button>
      </div>}
      <div className="sale-mobile-tabs" role="group" aria-label="Estado de las ventas de esta página">
        <button type="button" aria-pressed={mobileStatus === 'all'} onClick={() => setMobileStatus('all')}>Todas ({rows.length})</button>
        <button type="button" aria-pressed={mobileStatus === 'pending'} onClick={() => setMobileStatus('pending')}>Pendientes ({rows.filter(sale => sale.saldo_pendiente > 0).length})</button>
        <button type="button" aria-pressed={mobileStatus === 'paid'} onClick={() => setMobileStatus('paid')}>Pagadas ({rows.filter(sale => sale.saldo_pendiente === 0).length})</button>
        <small>Ventas de la página {page}</small>
      </div>
      {isAdmin && <div className="sale-mobile-summary-card">
        <div><span>Total de ventas</span><strong>{salesVisible ? (salesTotal === null ? '—' : money.format(salesTotal)) : 'Q ••••••'}</strong></div>
        <div><span>Ganancia</span><strong>{profitVisible ? (profitTotal === null ? '—' : money.format(profitTotal)) : 'Q ••••••'}</strong></div>
        <button type="button" className="sale-mobile-icon" aria-label={salesVisible && profitVisible ? 'Ocultar importes' : 'Mostrar importes'} aria-pressed={salesVisible && profitVisible} onClick={() => { const visible = !(salesVisible && profitVisible); setSalesVisible(visible); setProfitVisible(visible) }}>
          <svg width="23" height="23" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden="true"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" /><circle cx="12" cy="12" r="3" />{!(salesVisible && profitVisible) && <path d="m3 3 18 18" />}</svg>
        </button>
      </div>}
      {error && <div role="alert" className="sale-list-error"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div>}
      <div className="sale-list-body" aria-busy={loading}>
        {loading ? <p role="status">Cargando ventas…</p> : !error && rows.length === 0 ?
          <p role="status">{isAdmin && range ? 'No hay ventas para los filtros de esa fecha.' : 'No hay ventas para mostrar. La caja puede estar cerrada o aún no tener ventas.'}</p> : !error && <>
            {!mobileRows.length && <p className="sale-mobile-empty" role="status">No hay ventas que coincidan en esta página.</p>}
            <ul className="sale-mobile-cards" aria-label="Ventas">
              {mobileRows.map(sale => <li key={sale.id_ventas}>
                <button type="button" className={`sale-mobile-card${sale.saldo_pendiente > 0 ? ' is-pending' : ''}${selected?.id_ventas === sale.id_ventas ? ' is-selected' : ''}`}
                  disabled={saving} aria-label={`Ver detalle de venta ${sale.id_ventas}, cliente ${sale.cliente}`}
                  aria-expanded={selected?.id_ventas === sale.id_ventas}
                  onClick={() => { setSelected(sale); setSuccess('') }}>
                  <span className="sale-mobile-card-top">
                    <strong className="sale-mobile-card-client">{sale.cliente}</strong>
                    <strong className="sale-mobile-card-total">{money.format(sale.total)}</strong>
                  </span>
                  <span className="sale-mobile-card-date">Venta #{sale.id_ventas} · {Number.isNaN(Date.parse(sale.fecha_venta)) ? 'Fecha no disponible' : date.format(new Date(sale.fecha_venta))} · {sale.usuario}</span>
                  <span className="sale-mobile-card-bottom">
                    <span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.saldo_pendiente === 0 ? 'Pagada' : sale.estado}</span>
                    {sale.saldo_pendiente > 0 && <span className="sale-mobile-card-balance">Debe <strong>{money.format(sale.saldo_pendiente)}</strong></span>}
                  </span>
                </button>
              </li>)}
            </ul>
            <div className="sales-table-scroll" role="region" aria-label="Listado de ventas" tabIndex={0}>
              <table><caption className="sr-only">{isAdmin && range ? 'Ventas de la fecha seleccionada, de todas las sesiones de caja' : 'Ventas registradas en la sesión de caja abierta'}</caption>
                <thead><tr><th scope="col">Fecha y hora</th><th scope="col">Cliente</th><th scope="col">Responsable</th><th scope="col" className="sale-list-money">Total</th>{isAdmin && <th scope="col" className="sale-list-money">Ganancia</th>}<th scope="col" className="sale-list-money">Saldo pendiente</th><th scope="col">Estado</th></tr></thead>
                <tbody>{rows.map(sale => <tr key={sale.id_ventas} className={selected?.id_ventas === sale.id_ventas ? 'sale-list-row is-selected' : 'sale-list-row'}
                  tabIndex={saving ? -1 : 0} aria-label={'Ver detalle de la venta de ' + sale.cliente} aria-expanded={selected?.id_ventas === sale.id_ventas}
                  onKeyDown={event => { if (!saving && (event.key === 'Enter' || event.key === ' ')) { event.preventDefault(); setSelected(sale); setSuccess('') } }}
                  onClick={() => { if (!saving) { setSelected(sale); setSuccess('') } }}>
                  <td className="sale-list-nowrap">{Number.isNaN(Date.parse(sale.fecha_venta)) ? 'Fecha no disponible' : date.format(new Date(sale.fecha_venta))}</td>
                  <td>{sale.cliente}</td><td>{sale.usuario}</td><td className="sale-list-money">{money.format(sale.total)}</td>
                  {isAdmin && <td className="sale-list-money">{sale.ganancia_total === undefined ? 'No disponible' : money.format(sale.ganancia_total)}</td>}
                  <td className="sale-list-money">{money.format(sale.saldo_pendiente)}</td>
                  <td><span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.saldo_pendiente === 0 ? 'Pagada' : sale.estado}</span></td>
                </tr>)}</tbody>
              </table>
            </div>
          </>}
      </div>
      <footer className="sale-list-footer">
        <label className="sale-list-page-size">Filas por página
          <select value={pageSize} disabled={loading || saving} onChange={event => { setPageSize(Number(event.target.value)); setPage(1) }}>
            <option value={10}>10</option><option value={25}>25</option><option value={50}>50</option>
          </select>
        </label>
        {!error && (page > 1 || hasNext) && <nav className="sales-history-pagination" aria-label="Páginas de ventas">
          <button type="button" disabled={loading || saving || page === 1} onClick={() => setPage(v => v - 1)} aria-label="Página anterior" title="Página anterior"><span aria-hidden="true">←</span></button>
          <span>Página {page}</span>
          <button type="button" disabled={loading || saving || !hasNext} onClick={() => setPage(v => v + 1)} aria-label="Página siguiente" title="Página siguiente"><span aria-hidden="true">→</span></button>
        </nav>}
      </footer>
    </div>
  </section>
}
