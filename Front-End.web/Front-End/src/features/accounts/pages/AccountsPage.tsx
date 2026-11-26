import { useCallback, useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { requestJson } from '../../../shared/api/httpClient'
import { parseCashSession } from '../../cash/api/cashApi'
import { SalePaymentForm } from '../../sales/pages/SalePaymentForm'
import { PurchaseDetails } from '../../purchases/components/PurchaseDetails'
import { loadAccounts } from '../api/accountsApi'
import type { Account, AccountType, AccountsPage as AccountsResult } from '../api/accountsApi'
import '../../sales/pages/SalesPage.css'
import '../../purchases/pages/PurchasesPage.css'
import './AccountsPage.css'

const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
export function AccountsPage() {
  const { session, logout } = useAuth()
  const token = session?.token
  const allowed = session?.user.role === 'ADMINISTRADOR'
  const [desktopView, setDesktopView] = useState(() => window.matchMedia('(min-width: 801px)').matches)
  useEffect(() => {
    const media = window.matchMedia('(min-width: 801px)')
    const update = () => setDesktopView(media.matches)
    media.addEventListener('change', update)
    return () => media.removeEventListener('change', update)
  }, [])
  const [type, setType] = useState<AccountType>('cobrar')
  const [search, setSearch] = useState('')
  const [applied, setApplied] = useState('')
  const [page, setPage] = useState(1)
  const [data, setData] = useState<AccountsResult | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [revision, setRevision] = useState(0)
  const [selected, setSelected] = useState<Account | null>(null)
  const [cashOpen, setCashOpen] = useState(false)
  const [saving, setSaving] = useState(false)
  const unauthorized = useCallback(() => logout('La sesión ya no es válida.'), [logout])
  useEffect(() => {
    const timer = window.setTimeout(() => { setApplied(search.trim()); setPage(1) }, 300)
    return () => window.clearTimeout(timer)
  }, [search])
  useEffect(() => {
    if (!token || !allowed) return
    const controller = new AbortController()
    setLoading(true); setError(''); setCashOpen(false)
    async function load() {
      try {
        const [result, cash] = await Promise.all([
          loadAccounts(token!, type, page, applied, controller.signal),
          requestJson('/Caja/actual', { token, signal: controller.signal }).then(parseCashSession),
        ])
        if (!controller.signal.aborted) { setData(result); setCashOpen(cash !== null) }
      } catch (failure) {
        if (controller.signal.aborted) return
        setData(null)
        if (failure instanceof ApiError && failure.status === 401) unauthorized()
        else setError(failure instanceof Error ? failure.message : 'No se pudieron cargar las cuentas.')
      } finally { if (!controller.signal.aborted) setLoading(false) }
    }
    void load()
    return () => controller.abort()
  }, [token, allowed, type, page, applied, revision, unauthorized])
  if (!allowed) return null
  function paid() { setSelected(null); setRevision(v => v + 1) }
  return <section className="accounts-panel" aria-label="Cuentas">

    <div className="accounts-totals">
      <article><span>Por cobrar</span><strong>{!loading && data ? money.format(data.totalPorCobrar) : '—'}</strong></article>
      <article><span>Por pagar</span><strong>{!loading && data ? money.format(data.totalPorPagar) : '—'}</strong></article>
    </div>
    <div className="accounts-toolbar">
      <div className="accounts-tabs" aria-label="Tipo de cuenta">{(['cobrar', 'pagar'] as const).map(t => <button key={t} type="button" disabled={saving} aria-pressed={type === t}
        onClick={() => { setType(t); setPage(1); setSelected(null) }}>Por {t}</button>)}</div>
      <input aria-label={type === 'cobrar' ? 'Buscar cliente o número de venta' : 'Buscar proveedor o número de compra'} placeholder={type === 'cobrar' ? 'Cliente o número de venta' : 'Proveedor o número de compra'}
        maxLength={100} value={search} disabled={saving} onChange={e => setSearch(e.target.value)} />
      <RefreshButton label="Actualizar cuentas" loading={loading || saving} onClick={() => setRevision(v => v + 1)} />
    </div>

    {!loading && !error && !cashOpen && <p>Abre la caja para registrar pagos. Podés consultar los detalles.</p>}
    {error && <p role="alert">{error}</p>}
    {loading ? <p role="status">Cargando cuentas…</p> : !error && data && <>

      {data.items.length === 0 ? <p>No hay cuentas pendientes que coincidan.</p> : <div className="accounts-table-scroll">{desktopView ? <table>
        <thead><tr><th>{type === 'cobrar' ? 'Venta' : 'Compra'}</th><th>{type === 'cobrar' ? 'Cliente' : 'Proveedor'}</th><th>Fecha</th><th>Total</th><th>Abonado</th><th>Saldo</th><th>Detalle</th></tr></thead>
        <tbody>{data.items.map(a => <tr key={a.id} className={desktopView && !saving ? 'accounts-clickable-row' : undefined}
          tabIndex={desktopView && !saving ? 0 : undefined} aria-haspopup={desktopView ? 'dialog' : undefined}
          aria-label={desktopView ? `Ver detalle de ${type === 'cobrar' ? 'venta' : 'compra'} ${a.id}, ${a.nombre}` : undefined}
          onClick={event => { if (desktopView && !saving) { event.currentTarget.focus(); setSelected(a) } }}
          onKeyDown={event => {
            if (desktopView && !saving && event.target === event.currentTarget && !event.nativeEvent.isComposing && (event.key === 'Enter' || event.key === ' ')) {
              event.preventDefault(); setSelected(a)
            }
          }}>
          <td data-label={type === 'cobrar' ? 'Venta' : 'Compra'}>#{a.id}</td><td data-label={type === 'cobrar' ? 'Cliente' : 'Proveedor'}><strong>{a.nombre}</strong></td>
          <td data-label="Fecha">{date.format(new Date(a.fecha))}</td><td data-label="Total">{money.format(a.total)}</td><td data-label="Abonado">{money.format(a.total - a.saldo)}</td>
          <td data-label="Saldo" className="accounts-balance">{money.format(a.saldo)}</td><td><button type="button" className="accounts-detail-button" disabled={saving} tabIndex={desktopView ? -1 : undefined}
            aria-label={`Ver detalle de ${type === 'cobrar' ? 'venta' : 'compra'} ${a.id}`} aria-haspopup="dialog"
            onClick={event => { event.stopPropagation(); setSelected(a) }}>
            {desktopView ? <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m9 18 6-6-6-6" /></svg> : 'Ver detalle'}
          </button></td>
        </tr>)}</tbody>
      </table> : <ul className="accounts-mobile-cards" aria-label={type === 'cobrar' ? 'Cuentas por cobrar' : 'Cuentas por pagar'}>
        {data.items.map(a => <li key={a.id} className="accounts-mobile-card">
          <div className="accounts-mobile-main">
            <strong className="accounts-mobile-name">{a.nombre}</strong>
            <strong className="accounts-mobile-total">{money.format(a.total)}</strong>
            <span className="accounts-mobile-balance">Saldo <strong>{money.format(a.saldo)}</strong></span>
          </div>
          <div className="accounts-mobile-options">
            <span className="accounts-mobile-user">{a.usuario}</span>
            <button type="button" className="accounts-mobile-more" disabled={saving} aria-haspopup="dialog"
              aria-label={`Ver más detalles de ${type === 'cobrar' ? 'la venta' : 'la compra'} de ${a.nombre}`} title="Ver más detalles" onClick={() => setSelected(a)}>
              <svg width="22" height="22" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true"><circle cx="12" cy="5" r="2" /><circle cx="12" cy="12" r="2" /><circle cx="12" cy="19" r="2" /></svg>
            </button>
            <span className="accounts-mobile-date">{date.format(new Date(a.fecha))}</span>
          </div>
        </li>)}
      </ul>}</div>}
      <nav className="accounts-pagination" aria-label={`Páginas de cuentas, página actual ${page}`}>
        <button type="button" aria-label="Página anterior" title="Página anterior" disabled={page === 1 || saving} onClick={() => setPage(v => v - 1)}>
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m15 18-6-6 6-6" /></svg>
        </button>
        <button type="button" aria-label="Página siguiente" title="Página siguiente" disabled={!data.hayMas || saving} onClick={() => setPage(v => v + 1)}>
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true"><path d="m9 18 6-6-6-6" /></svg>
        </button>
      </nav>
    </>}
    {selected && type === 'cobrar' && <SalePaymentForm key={selected.id} sale={{ id_ventas: selected.id, cliente: selected.nombre, usuario: selected.usuario, subtotal: selected.subtotal, descuento: selected.descuento,
      total: selected.total, ganancia_total: selected.ganancia ?? undefined, saldo_pendiente: selected.saldo, estado: 'Pendiente', fecha_venta: selected.fecha }} onClose={() => setSelected(null)} onPaid={paid} onSaving={setSaving} />}
    {selected && type === 'pagar' && token && <PurchaseDetails key={selected.id} purchase={{ id_compra: selected.id, nombre_proveedor: selected.nombre, nombre_usuario: selected.usuario,
      total_compra: selected.total, saldo_pendiente: selected.saldo, descripcion_estado_compra: 'Pendiente', fecha_ingreso: selected.fecha }} token={token} canPay={cashOpen} onClose={() => setSelected(null)} onPaid={paid} onUnauthorized={unauthorized} />}
  </section>
}
