const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../../Front-End.web/Front-End');
function edit(file, before, after) {
  const target = path.join(root, file);
  const source = fs.readFileSync(target, 'utf8');
  if (!source.includes(before)) throw new Error('No se encontró el texto esperado: ' + file);
  fs.writeFileSync(target, source.replace(before, after));
}
edit('src/app/navigation/navigationItems.ts', "'ventas' | 'caja'", "'ventas' | 'listado-ventas' | 'caja'");
edit('src/app/navigation/navigationItems.ts', "{ id: 'ventas', label: 'Ventas', group: 'Operación', description: 'Nueva venta, historial y operaciones autorizadas.', roles: staff },", "{ id: 'ventas', label: 'Nueva venta', group: 'Operación', description: 'Registrar una venta.', roles: staff },\n  { id: 'listado-ventas', label: 'Ventas', group: 'Operación', description: 'Ventas registradas en la caja abierta.', roles: staff },");
edit('src/app/navigation/NavigationIcon.tsx', "  ventas:", "  'listado-ventas': 'M5 3h14v18l-3-2-4 2-4-2-3 2V3ZM8 8h8M8 12h8M8 16h4',\n  ventas:");
edit('src/app/layouts/StaffLayout.tsx', "import { SalesPage }", "import { SaleListPage } from '../../features/sales/pages/SaleListPage'\nimport { SalesPage }");
edit('src/app/layouts/StaffLayout.tsx', "{current.id === 'ventas' ?", "{current.id === 'listado-ventas' ? <SaleListPage key={session.user.id} /> : current.id === 'ventas' ?");
edit('src/features/sales/api/salesApi.ts', '`/Venta/listar?pagina=${page}&tamanoPagina=100`', '`/Venta/listar?pagina=${page}&tamanoPagina=50`');
fs.writeFileSync(path.join(root, 'src/features/sales/pages/SaleListPage.tsx'), `import { useEffect, useState } from 'react'
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
  const [page, setPage] = useState(1)
  const [revision, setRevision] = useState(0)
  const [rows, setRows] = useState<SaleHistory[]>([])
  const [hasNext, setHasNext] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!token) return
    const controller = new AbortController()
    setLoading(true); setError(''); setRows([]); setHasNext(false)
    async function load() {
      try {
        const result = await loadSaleHistory(token!, controller.signal, page)
        // La API devuelve un arreglo sin total; consultar la siguiente página evita
        // habilitar Siguiente cuando la última página tiene exactamente 50 ventas.
        const next = result.length === 50 ? await loadSaleHistory(token!, controller.signal, page + 1) : []
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
  }, [token, page, revision, logout])

  return <section className="sales-page sale-list-page" aria-label="Ventas de la caja abierta">
    <div className="sales-card">
      <div className="sales-heading">
        <div><h2>Caja abierta</h2><p className="sale-list-description">50 ventas por página · Más recientes primero</p></div>
        <RefreshButton label="Actualizar ventas" loading={loading} onClick={() => { setPage(1); setRevision(v => v + 1) }} />
      </div>
      {error && <div role="alert" className="sale-list-error"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div>}
      <div aria-busy={loading}>
        {loading ? <p role="status">Cargando ventas…</p> : !error && rows.length === 0 ?
          <p role="status">No hay ventas para mostrar. La caja puede estar cerrada o aún no tener ventas.</p> : !error && <>
            <div className="sales-table-scroll" role="region" aria-label="Listado de ventas" tabIndex={0}>
              <table><caption className="sale-list-caption">Ventas registradas en la sesión de caja abierta</caption>
                <thead><tr><th scope="col">Venta</th><th scope="col">Fecha y hora</th><th scope="col">Cliente</th><th scope="col">Total</th><th scope="col">Saldo pendiente</th><th scope="col">Estado</th></tr></thead>
                <tbody>{rows.map(sale => <tr key={sale.id_ventas}>
                  <td>#{sale.id_ventas}</td><td className="sale-list-nowrap">{Number.isNaN(Date.parse(sale.fecha_venta)) ? 'Fecha no disponible' : date.format(new Date(sale.fecha_venta))}</td>
                  <td>{sale.cliente}</td><td className="sale-list-money">{money.format(sale.total)}</td>
                  <td className="sale-list-money">{money.format(sale.saldo_pendiente)}</td>
                  <td><span className={sale.saldo_pendiente === 0 ? 'sale-list-status is-paid' : 'sale-list-status'}>{sale.estado}</span></td>
                </tr>)}</tbody>
              </table>
            </div>
          </>}
      </div>
      <nav className="sales-history-pagination" aria-label="Páginas de ventas">
        <button type="button" disabled={loading || page === 1} onClick={() => setPage(v => v - 1)}>Anterior</button>
        <span role="status">Página {page}</span>
        <button type="button" disabled={loading || !!error || !hasNext} onClick={() => setPage(v => v + 1)}>Siguiente</button>
      </nav>
    </div>
  </section>
}
`);
fs.writeFileSync(path.join(root, 'src/features/sales/pages/SaleListPage.css'), `.sale-list-description { margin: 0 0 1rem; color: var(--text-muted); font-size: .9rem; }
.sale-list-caption { text-align: left; padding: 1rem 0; color: var(--text-muted); }
.sale-list-page .sales-history-pagination { margin-top: 1.5rem; flex-wrap: wrap; }
.sale-list-nowrap, .sale-list-money { white-space: nowrap; }
.sale-list-page th:nth-child(4), .sale-list-page th:nth-child(5), .sale-list-money { text-align: right; font-variant-numeric: tabular-nums; }
.sale-list-status { display: inline-block; padding: .25rem .6rem; border-radius: var(--radius); background: var(--surface-tint); white-space: nowrap; }
.sale-list-status.is-paid { color: #166534; background: #dcfce7; }
.sale-list-error { border: 1px solid var(--border); padding: 1rem; border-radius: var(--radius); }
`);
