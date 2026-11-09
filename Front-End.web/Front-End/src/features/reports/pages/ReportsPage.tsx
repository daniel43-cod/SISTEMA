import { useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { getSalesReport, getPurchasesReport, getExpensesReport, listReportSessions } from '../api/reportsApi'
import type { ReportSession, SalesReport, PurchasesReport, ExpensesReport } from '../api/reportsApi'
import './ReportsPage.css'
const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
function SalesChart({ report, purchasesReport, expensesReport }: { report: SalesReport
  purchasesReport: PurchasesReport | null
  expensesReport: ExpensesReport | null
}) {
const [hovered, setHovered] = useState<{
  index: number
  series: 'venta' | 'ganancia' | 'compra' | 'gasto'
  
} | null>(null)
  const points = report.puntos.filter(p => p.idVenta !== null)
 const purchasePoints =
  purchasesReport?.idSesionCaja === report.idSesionCaja
    ? purchasesReport.puntos
    : []
  const expensePoints = expensesReport?.idSesionCaja === report.idSesionCaja ? expensesReport.puntos : []
 // const max = points.reduce((maximum, p) => Math.max(maximum, p.montoVenta), 1)
 const max = points.reduce(
  (maximum, p) => Math.max(maximum, p.montoVenta, p.gananciaVenta),
  purchasePoints.reduce((maximum, p) => Math.max(maximum, p.montoCompra), expensePoints.reduce((maximum, p) => Math.max(maximum, p.montoGasto), 1)),
)

const min = points.reduce(
  (minimum, p) => Math.min(minimum, p.montoVenta, p.gananciaVenta),
  purchasePoints.reduce((minimum, p) => Math.min(minimum, p.montoCompra), expensePoints.reduce((minimum, p) => Math.min(minimum, p.montoGasto), 0)),
)
  const beginning = Date.parse(report.apertura)
  const ending = Math.max(beginning + 1, Date.parse(report.hasta), ...purchasePoints.map(p => Date.parse(p.fecha)), ...expensePoints.map(p => Date.parse(p.fecha)))
  const xTime = (time: number) => 100 + (time - beginning) / (ending - beginning) * 820
  const x = (i: number) => xTime(Date.parse(points[i]!.fecha))
 // const y = (value: number) => 270 - value / max * 220
  const purchaseX = (i: number) => xTime(Date.parse(purchasePoints[i]!.fecha))
  const expenseX = (i: number) => xTime(Date.parse(expensePoints[i]!.fecha))
  const y = (value: number) => 270 - (value - min) / (max - min) * 220
  function hoverPoint(
  clientX: number,
  clientY: number,
  svg: SVGSVGElement,
) {
  const matrix = svg.getScreenCTM()
  if (!matrix) return

  const cursor = new DOMPoint(clientX, clientY)
    .matrixTransform(matrix.inverse())

  let nearest: typeof hovered = null
  let distance = 14 * 14

  points.forEach((p, i) => {
    const series = [
      { name: 'venta' as const, value: p.montoVenta },
      { name: 'ganancia' as const, value: p.gananciaVenta },
    ]

    series.forEach(item => {
      const next =
        (x(i) - cursor.x) ** 2 +
        (y(item.value) - cursor.y) ** 2

      if (next <= distance) {
        nearest = { index: i, series: item.name }
        distance = next
      }
    })
  })

  purchasePoints.forEach((p, i) => {
    const next = (purchaseX(i) - cursor.x) ** 2 + (y(p.montoCompra) - cursor.y) ** 2
    if (next <= distance) {
      nearest = { index: i, series: 'compra' }
      distance = next
    }
  })

  expensePoints.forEach((p, i) => {
    const next = (expenseX(i) - cursor.x) ** 2 + (y(p.montoGasto) - cursor.y) ** 2
    if (next <= distance) {
      nearest = { index: i, series: 'gasto' }
      distance = next
    }
  })
  setHovered(nearest)
}
  const path = points.map((p, i) => (i === 0 ? 'M' : ' L') + x(i) + ',' + y(p.montoVenta)).join('')
  const profitPath = points
  .map(
    (p, i) =>
      (i === 0 ? 'M' : ' L') + x(i) + ',' + y(p.gananciaVenta),
  )
  .join('')
if (points.length === 0 && purchasePoints.length === 0 && expensePoints.length === 0) {
  return <p>No hay ventas, compras ni gastos para mostrar en la gráfica.</p>
}
const purchasePath = purchasePoints
  .map(
    (p, i) =>
      (i === 0 ? 'M' : ' L') +
      purchaseX(i) +
      ',' +
      y(p.montoCompra),
  )
  .join('')

const expensePath = expensePoints.map((p, i) => (i === 0 ? 'M' : ' L') + expenseX(i) + ',' + y(p.montoGasto)).join('')
const hoveredExpense = hovered?.series === 'gasto' ? expensePoints[hovered.index] : undefined
const hoveredSale = hovered && (hovered.series === 'venta' || hovered.series === 'ganancia') ? points[hovered.index] : undefined
const hoveredPurchase = hovered?.series === 'compra' ? purchasePoints[hovered.index] : undefined
const hoveredPoint = hoveredExpense ?? hoveredSale ?? hoveredPurchase
const hoveredValue = hoveredExpense ? hoveredExpense.montoGasto : hoveredPurchase
  ? hoveredPurchase.montoCompra
  : hoveredSale
    ? hovered?.series === 'ganancia' ? hoveredSale.gananciaVenta : hoveredSale.montoVenta
    : 0
const hoveredX = hoveredPoint ? xTime(Date.parse(hoveredPoint.fecha)) : 0
const hoveredColor = hovered?.series === 'gasto' ? '#dc2626' : hovered?.series === 'compra'
  ? '#ea580c'
  : hovered?.series === 'ganancia' ? '#15803d' : '#07558e'
const hoveredLabel = hovered?.series === 'compra' || hovered?.series === 'gasto'
  ? ''
  : hovered?.series === 'ganancia' ? 'Ganancia' : 'Total de venta'
  return <div className="report-chart">
    <svg viewBox="0 0 960 340" role="img" aria-label="Ventas, ganancias, compras y gastos individuales de la sesión">
      {/* Escala vertical compartida por los importes y las ganancias de las ventas. */}
      {[0, 1, 2, 3, 4].map(i => <g key={i}><line x1="100" x2="920" y1={y(min + (max - min) * i / 4)} y2={y(min + (max - min) * i / 4)} stroke="#dce5ee" /><text x="90" y={y(min + (max - min) * i / 4) + 4} textAnchor="end">{money.format(min + (max - min) * i / 4)}</text></g>)}
      <path d={path} fill="none" stroke="#0876bc" strokeWidth="3" strokeLinejoin="round" />
      {/* Línea verde: ganancia individual de cada venta, sin acumular. */}
      <path d={profitPath} fill="none" stroke="#16a34a" strokeWidth="3" strokeLinejoin="round" />
      {points.length <= 60 && points.map((p, i) => (
        <circle key={`profit-${p.idVenta}`} cx={x(i)} cy={y(p.gananciaVenta)} r="3" fill="#16a34a" />
      ))}
      {points.length <= 60 && points.map((p, i) => <circle key={p.idVenta ?? 'opening'} cx={x(i)} cy={y(p.montoVenta)} r="3" fill="#0876bc" />)}
      {[0, 1, 2, 3, 4].map(i => <text key={i} x={100 + i * 205} y="302" textAnchor="middle">{date.format(new Date(beginning + (ending - beginning) * i / 4))}</text>)}
      {/* Compras individuales de la misma sesión de caja. */}
<path
  d={purchasePath}
  fill="none"
  stroke="#ea580c"
  strokeWidth="3"
  strokeLinejoin="round"
/>

{purchasePoints.length <= 60 &&
  purchasePoints.map((p, i) => (
    <circle
      key={`purchase-${p.idCompra}`}
      cx={purchaseX(i)}
      cy={y(p.montoCompra)}
      r="3"
      fill="#ea580c"
    />
  ))}
      <path d={expensePath} fill="none" stroke="#dc2626" strokeWidth="3" strokeLinejoin="round" />
      {expensePoints.length <= 60 && expensePoints.map((p, i) => (
        <circle key={`expense-${p.idGasto}`} cx={expenseX(i)} cy={y(p.montoGasto)} r="3" fill="#dc2626" />
      ))}
      <rect x="86" y="26" width="848" height="258" fill="transparent"
        onPointerMove={event => hoverPoint(event.clientX, event.clientY, event.currentTarget.ownerSVGElement!)}
        onPointerDown={event => hoverPoint(event.clientX, event.clientY, event.currentTarget.ownerSVGElement!)}
        onPointerLeave={() => setHovered(null)} />
        {hovered && hoveredPoint && (
  <g pointerEvents="none" aria-hidden="true">
    <circle
      cx={hoveredX}
      cy={y(hoveredValue)}
      r="6"
      fill={hoveredColor}
      stroke="white"
      strokeWidth="2"
    />

    <g
      transform={`translate(
        ${Math.min(810, Math.max(210, hoveredX))},
        ${Math.max(30, y(hoveredValue) - 24)}
      )`}
    >
      <rect
        x="-110"
        y="-22"
        width="220"
        height="32"
        rx="6"
        fill={hoveredColor}
      />

      <text
        textAnchor="middle"
        y="-2"
        style={{ fill: '#fff', fontWeight: 600 }}
      >
        {hoveredLabel ? hoveredLabel + ': ' : ''}{money.format(hoveredValue)}
      </text>
    </g>
  </g>
)}
    </svg>
  </div>
}
export function ReportsPage() {
  const [expensesReport, setExpensesReport] = useState<ExpensesReport | null>(null)
  const [expensesLoading, setExpensesLoading] = useState(false)
  const [expensesError, setExpensesError] = useState('')
  const [purchasesReport, setPurchasesReport] =
  useState<PurchasesReport | null>(null)

const [purchasesLoading, setPurchasesLoading] = useState(false)
const [purchasesError, setPurchasesError] = useState('')
  const { session, logout } = useAuth()
  const token = session?.token
  const admin = session?.user.role === 'ADMINISTRADOR'
  const [sessions, setSessions] = useState<ReportSession[]>([])
  const [sessionId, setSessionId] = useState(0)
  const [sessionPage, setSessionPage] = useState(1)
  const [more, setMore] = useState(false)
  const [sessionLoading, setSessionLoading] = useState(true)
  const [sessionError, setSessionError] = useState('')
  const [revision, setRevision] = useState(0)
  const [report, setReport] = useState<SalesReport | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  useEffect(() => {
    if (!token || !admin) return
    const controller = new AbortController()
    setSessionLoading(true); setSessionError('')
    void listReportSessions(token, sessionPage, controller.signal).then(rows => {
      if (controller.signal.aborted) return
      setSessions(previous => sessionPage === 1 ? rows : [...previous.filter(p => !rows.some(s => s.id_sesion_caja === p.id_sesion_caja)), ...rows])
      setMore(rows.length === 50)
      if (sessionPage === 1) setSessionId(current => rows.some(s => s.id_sesion_caja === current) ? current : (rows.find(s => s.fecha_cierre === null)?.id_sesion_caja ?? rows[0]?.id_sesion_caja ?? 0))
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setSessionError(failure instanceof Error ? failure.message : 'No se pudieron cargar las sesiones.')
    }).finally(() => { if (!controller.signal.aborted) setSessionLoading(false) })
    return () => controller.abort()
  }, [token, admin, sessionPage, revision, logout])
  useEffect(() => {
    if (!token || !admin || !sessionId) return
    const controller = new AbortController()
    setLoading(true); setError(''); setReport(null)
    void getSalesReport(token, sessionId, controller.signal).then(value => { if (!controller.signal.aborted) setReport(value) }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setError(failure instanceof Error ? failure.message : 'No se pudo cargar el reporte.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [token, admin, sessionId, revision, logout])
  useEffect(() => {
    if (!token || !admin || !sessionId) return
    const controller = new AbortController()
    setPurchasesLoading(true)
    setPurchasesError('')
    setPurchasesReport(null)
    void getPurchasesReport(token, controller.signal).then(value => {
      if (controller.signal.aborted) return
      // No mezclar las compras de la caja activa con ventas de otra sesión.
      setPurchasesReport(value?.idSesionCaja === sessionId ? value : null)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setPurchasesError(failure instanceof Error ? failure.message : 'No se pudo cargar el reporte de compras.')
    }).finally(() => {
      if (!controller.signal.aborted) setPurchasesLoading(false)
    })
    return () => controller.abort()
  }, [token, admin, sessionId, revision, logout])
  useEffect(() => {
    if (!token || !admin || !sessionId) return
    const controller = new AbortController()
    setExpensesLoading(true)
    setExpensesError('')
    setExpensesReport(null)
    void getExpensesReport(token, controller.signal).then(value => {
      if (controller.signal.aborted) return
      // Solo mostrar gastos de la misma sesión seleccionada.
      setExpensesReport(value?.idSesionCaja === sessionId ? value : null)
    }).catch(failure => {
      if (controller.signal.aborted) return
      if (failure instanceof ApiError && failure.status === 401) logout('La sesión ya no es válida.')
      else setExpensesError(failure instanceof Error ? failure.message : 'No se pudo cargar el reporte de gastos.')
    }).finally(() => {
      if (!controller.signal.aborted) setExpensesLoading(false)
    })
    return () => controller.abort()
  }, [token, admin, sessionId, revision, logout])
  if (!admin) return null
  return <section className="reports-panel" aria-labelledby="sales-report-title">
    <div className="reports-heading"><h2 id="sales-report-title">Importe de ventas por sesión de caja</h2><RefreshButton label="Actualizar reporte" loading={loading || sessionLoading || purchasesLoading || expensesLoading} onClick={() => { setSessionPage(1); setRevision(v => v + 1) }} /></div>
    <div className="reports-filters"><label>Sesión de caja<select value={sessionId || ''} disabled={sessionLoading || !sessions.length} onChange={e => setSessionId(Number(e.target.value))}><option value="" disabled>Selecciona una sesión</option>{sessions.map(s => <option key={s.id_sesion_caja} value={s.id_sesion_caja}>{date.format(new Date(s.fecha_apertura))} · {s.fecha_cierre === null ? 'Abierta' : 'Cerrada'} · {s.usuario_apertura}</option>)}</select></label>
      {more && <button type="button" disabled={sessionLoading} onClick={() => setSessionPage(p => p + 1)}>Cargar sesiones anteriores</button>}
    </div>
    {sessionError && <p role="alert">{sessionError}</p>}
    {sessionLoading && <p role="status">Cargando sesiones…</p>}
    {!sessionLoading && !sessionError && !sessions.length && <p>No hay sesiones de caja registradas.</p>}
    {loading ? <p role="status">Cargando gráfica…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> : report && <>
      <p className={'reports-session-status ' + (report.cierre === null ? 'is-open' : 'is-closed')}>
        <span aria-hidden="true" />{report.cierre === null ? 'Abierta' : 'Cerrada'}
      </p>
      <div className="reports-summary-grid">
        <article className="reports-summary-card reports-summary-sales">
          <span className="reports-summary-label"><i aria-hidden="true" />Ventas</span>
          <strong>{money.format(report.totalVentas)}</strong>
          <small>{report.cantidadVentas} {report.cantidadVentas === 1 ? 'venta' : 'ventas'}</small>
        </article>
        <article className="reports-summary-card reports-summary-profit">
          <span className="reports-summary-label"><i aria-hidden="true" />Ganancia</span>
          <strong>{money.format(report.puntos.filter(p => p.idVenta !== null).reduce((sum, p) => sum + p.gananciaVenta, 0))}</strong>
          <small>Margen {report.totalVentas > 0 ? Math.round(report.puntos.filter(p => p.idVenta !== null).reduce((sum, p) => sum + p.gananciaVenta, 0) / report.totalVentas * 100) + ' %' : '—'}</small>
        </article>
        <article className="reports-summary-card reports-summary-purchases">
          <span className="reports-summary-label"><i aria-hidden="true" />Compras</span>
          <strong>{purchasesLoading ? '…' : purchasesReport?.idSesionCaja === report.idSesionCaja ? money.format(purchasesReport.totalCompras) : '—'}</strong>
          <small>{purchasesLoading ? 'Cargando…' : purchasesError ? 'No disponible' : purchasesReport?.idSesionCaja === report.idSesionCaja
            ? purchasesReport.cantidadCompras + (purchasesReport.cantidadCompras === 1 ? ' compra' : ' compras') : 'Solo caja activa'}</small>
        </article>
        <article className="reports-summary-card reports-summary-expenses">
          <span className="reports-summary-label"><i aria-hidden="true" />Gastos</span>
          <strong>{expensesLoading ? '…' : expensesReport?.idSesionCaja === report.idSesionCaja ? money.format(expensesReport.totalGastos) : '—'}</strong>
          <small>{expensesLoading ? 'Cargando…' : expensesError ? 'No disponible' : expensesReport?.idSesionCaja === report.idSesionCaja
            ? expensesReport.cantidadGastos + (expensesReport.cantidadGastos === 1 ? ' gasto' : ' gastos') : 'Solo caja activa'}</small>
        </article>
      </div>
      
      {report.cantidadVentas === 0 && <p>No hay ventas registradas en esta sesión.</p>}
      <SalesChart key={sessionId} report={report} purchasesReport={purchasesReport} expensesReport={expensesReport} />
    </>}
    {sessionId > 0 && (purchasesLoading ? (
      <p role="status">Cargando compras…</p>
    ) : purchasesError ? (
      <div role="alert">
        <p>{purchasesError}</p>
        <button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button>
      </div>
    ) : purchasesReport && purchasesReport.idSesionCaja === sessionId ? null : (
      <p>El reporte de compras está disponible únicamente para la caja activa.</p>
    ))}
    {sessionId > 0 && (expensesLoading ? (
      <p role="status">Cargando gastos…</p>
    ) : expensesError ? (
      <div role="alert">
        <p>{expensesError}</p>
        <button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button>
      </div>
    ) : expensesReport?.idSesionCaja === sessionId ? null : (
      <p>El reporte de gastos está disponible únicamente para la caja activa.</p>
    ))}
  </section>
}
