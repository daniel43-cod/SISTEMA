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
