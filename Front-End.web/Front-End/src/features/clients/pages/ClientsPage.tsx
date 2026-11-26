import { EditClientDialog } from './EditClientDialog'
import { lastSevenSaleDays } from '../../sales/utils/saleDateRange'
import { ClientSaleDetails } from './ClientSaleDetails'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { loadClients, loadClientSummary, loadClientSales } from '../api/clientsApi'
import type { Client, ClientPage, ClientSummary, ClientSalesPage, ClientSale } from '../api/clientsApi'
import '../../sales/pages/SalesPage.css'
import './ClientsPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short' })
function Pages({ page, more, loading, onChange }: { page: number; more: boolean; loading: boolean; onChange: (page: number) => void }) {
  return page > 1 || more ? <nav className="clients-pagination" aria-label="Páginas">
    <button type="button" aria-label="Página anterior" disabled={loading || page === 1} onClick={() => onChange(page - 1)}>←</button>
    <span>Página {page}</span><button type="button" aria-label="Página siguiente" disabled={loading || !more} onClick={() => onChange(page + 1)}>→</button>
  </nav> : null
}
export function ClientsPage() {
  const { session, logout } = useAuth()
  const token = session?.token
  const allowed = session?.user.role === 'ADMINISTRADOR'
  const [search, setSearch] = useState('')
  const [filter, setFilter] = useState({ search: '', page: 1 })
  const [data, setData] = useState<ClientPage<Client> | null>(null)
  const [selected, setSelected] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const fail = useCallback((e: unknown) => {
    if (e instanceof ApiError && e.status === 401) logout('La sesión ya no es válida.')
    return e instanceof Error ? e.message : 'No se pudo consultar la información de clientes.'
  }, [logout])
  useEffect(() => {
    const timer = window.setTimeout(() => setFilter(f => f.search === search.trim() ? f : { search: search.trim(), page: 1 }), 300)
    return () => window.clearTimeout(timer)
  }, [search])
  useEffect(() => {
    if (!token || !allowed || selected !== null) return
    const abort = new AbortController()
    setLoading(true); setError(''); setData(null)
    void loadClients(token, filter.page, filter.search, abort.signal).then(result => { if (!abort.signal.aborted) setData(result) })
      .catch(e => { if (!abort.signal.aborted) setError(fail(e)) })
      .finally(() => { if (!abort.signal.aborted) setLoading(false) })
    return () => abort.abort()
  }, [token, allowed, filter, revision, selected, fail])
  if (!allowed || !token) return null
  if (selected !== null) return <ClientDetails key={selected} id={selected} token={token} fail={fail} onBack={() => setSelected(null)} />
  return <section className="clients-panel clients-list-panel" aria-label="Clientes">
    <div className="clients-toolbar"><input type="search" aria-label="Buscar clientes por nombre, NIT o teléfono" placeholder="Buscar cliente " maxLength={100} value={search} onChange={e => setSearch(e.target.value)} />
      <RefreshButton label="Actualizar clientes" loading={loading} onClick={() => setRevision(v => v + 1)} /></div>
    {error && <p role="alert">{error}</p>}
    {loading ? <p role="status">Cargando clientes…</p> : data && <>
      {data.items.length === 0 ? <p>No hay clientes que coincidan con la búsqueda.</p> : <div className="clients-table-scroll"><table>
        <thead><tr><th>Cliente</th><th>NIT</th><th>Teléfono</th><th>Dirección</th></tr></thead>
        <tbody>{data.items.map(c => <tr key={c.idCliente}><td data-label="Cliente"><button type="button" className="clients-name" onClick={() => setSelected(c.idCliente)}>{c.nombre}</button></td>
          <td className="clients-list-nit" data-label="NIT">{c.nit || '—'}</td><td data-label="Teléfono">{c.telefono || '—'}</td><td data-label="Dirección">{c.direccion || '—'}</td>
</tr>)}</tbody>
      </table></div>}
      <Pages page={filter.page} more={data.hayMas} loading={loading} onChange={page => setFilter(f => ({ ...f, page }))} />
    </>}
  </section>
}
function ClientDetails({ id, token, fail, onBack }: { id: number; token: string; fail: (e: unknown) => string; onBack: () => void }) {
  const [editing, setEditing] = useState(false)
  const [selectedSale, setSelectedSale] = useState<ClientSale | null>(null)
  const heading = useRef<HTMLHeadingElement>(null)
  const [summary, setSummary] = useState<ClientSummary | null>(null)
  const [sales, setSales] = useState<ClientSalesPage | null>(null)
  const [page, setPage] = useState(1)
  const [selectedDate, setSelectedDate] = useState('')
  const today = lastSevenSaleDays().hasta
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  useEffect(() => { heading.current?.focus() }, [])
  useEffect(() => {
    const abort = new AbortController()
    setLoading(true); setError(''); setSales(null)
    void Promise.all([loadClientSummary(token, id, abort.signal), loadClientSales(token, id, page, abort.signal, selectedDate)])
      .then(([s, h]) => { if (!abort.signal.aborted) { setSummary(s); setSales(h) } })
      .catch(e => { if (!abort.signal.aborted) { setSummary(null); setError(fail(e)) } })
      .finally(() => { if (!abort.signal.aborted) setLoading(false) })
    return () => abort.abort()
  }, [id, token, page, selectedDate, fail])
  const c = summary?.cliente
  return <section className="clients-panel" aria-label="Detalle del cliente">
    <div className="clients-toolbar clients-detail-heading">
    <h2 ref={heading} tabIndex={-1}>{c ? [c.nombre, c.apellido].filter(Boolean).join(' ') : 'Detalle del cliente'}</h2>{summary && !loading && <button type="button" className="clients-edit-trigger" onClick={() => setEditing(true)} aria-haspopup="dialog">Editar</button>}<button type="button" className="clients-back-icon" onClick={onBack} aria-label="Regresar a clientes" title="Regresar a clientes"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m12 19-7-7 7-7M5 12h14" /></svg></button></div>
    {error && <p role="alert">{error}</p>}
    {loading && <p role="status">Cargando historial…</p>}
    {summary && c && <>
      <dl className="clients-personal">{[['NIT', c.nit], ['DPI', c.dpi], ['Teléfono', c.telefono], ['Correo', c.correo_electronico], ['Dirección', c.direccion], ['Estado', summary.estado ? 'Activo' : 'Inactivo']].map(([label, value]) => <div key={label}><dt>{label}</dt><dd>{value || '—'}</dd></div>)}</dl>
      <div className="clients-summary"><article><span>Total comprado</span><strong>{money.format(summary.totalComprado)}</strong><small>{summary.cantidadVentas} ventas</small></article>
        <article><span>Total pagado</span><strong>{money.format(summary.totalPagado)}</strong></article>
        <article><span>Saldo pendiente</span><strong>{money.format(summary.saldoPendiente)}</strong></article></div>
    </>}
    <div className="clients-history-toolbar"><div className="clients-history-title"><h3><span className="clients-history-desktop-title">Historial de ventas</span><span className="clients-history-mobile-title">Historial</span></h3>{!selectedDate && <span className="clients-history-period">últimos 30 días</span>}</div><label className="clients-history-date" title={selectedDate || 'Filtrar por fecha'}><span className="clients-history-date-label">Fecha</span><svg className="clients-history-calendar-icon" width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 11h18" /></svg><input type="date" aria-label="Filtrar ventas del cliente por fecha" onClick={e => { if (window.matchMedia('(max-width: 800px)').matches) { try { e.currentTarget.showPicker?.() } catch { /* El control nativo sigue disponible. */ } } }} value={selectedDate} max={today} onChange={e => { const value = e.target.value; if (value && value > today) return; setSelectedDate(value); setPage(1); setSelectedSale(null) }} /></label></div>

    {!loading && sales && <>{sales.items.length === 0 ? <p>No hay ventas para la fecha o período seleccionado.</p> : <div className="clients-table-scroll"><table>
      <thead><tr><th>Venta</th><th>Fecha</th><th>Responsable</th><th>Total</th><th>Pagado</th><th>Saldo</th><th>Estado</th></tr></thead>
      <tbody>{sales.items.map(s => <tr key={s.idVenta} className="clients-sale-row" tabIndex={0} aria-haspopup="dialog" aria-label={`Ver detalles de venta ${s.idVenta}`} onClick={event => { event.currentTarget.focus(); setSelectedSale(s) }} onKeyDown={event => { if (event.target === event.currentTarget && !event.nativeEvent.isComposing && (event.key === 'Enter' || event.key === ' ')) { event.preventDefault(); setSelectedSale(s) } }}><td data-label="Venta">#{s.idVenta}</td><td data-label="Fecha">{date.format(new Date(s.fecha))}</td><td data-label="Responsable"><span className="clients-responsible-full">{s.responsable}</span><span className="clients-responsible-name">{s.nombreResponsable ?? s.responsable}</span></td>
        <td data-label="Total">{money.format(s.total)}</td><td data-label="Pagado">{money.format(s.pagado)}</td><td data-label="Saldo">{money.format(s.saldo)}</td><td data-label="Estado">{s.estado}</td>
        </tr>)}</tbody>
    </table></div>}<Pages page={page} more={sales.hayMas} loading={loading} onChange={setPage} /></>}
    {editing && summary && <EditClientDialog summary={summary} token={token} fail={fail} onClose={() => setEditing(false)} onSaved={updated => { setSummary(updated); setEditing(false) }} />}
    {selectedSale && <ClientSaleDetails key={selectedSale.idVenta} sale={selectedSale} clientName={c ? [c.nombre, c.apellido].filter(Boolean).join(' ') : ''} token={token} fail={fail} onClose={() => setSelectedSale(null)} />}
  </section>
}

