import { useEffect, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { getSalesReport, listReportSessions } from '../api/reportsApi'
import type { ReportSession, SalesReport } from '../api/reportsApi'
import './ReportsPage.css'
const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
function SalesChart({ report }: { report: SalesReport }) {
const [hovered, setHovered] = useState<{
  index: number
  series: 'venta' | 'ganancia'
} | null>(null)
  const points = report.puntos.filter(p => p.idVenta !== null)
 // const max = points.reduce((maximum, p) => Math.max(maximum, p.montoVenta), 1)
 const max = points.reduce(
  (maximum, p) => Math.max(maximum, p.montoVenta, p.gananciaVenta),
  1,
)

const min = points.reduce(
  (minimum, p) => Math.min(minimum, p.montoVenta, p.gananciaVenta),
  0,
)
  const beginning = Date.parse(report.apertura)
  const ending = Math.max(beginning + 1, Date.parse(report.hasta))
  const xTime = (time: number) => 100 + (time - beginning) / (ending - beginning) * 820
  const x = (i: number) => xTime(Date.parse(points[i]!.fecha))
 // const y = (value: number) => 270 - value / max * 220
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

  setHovered(nearest)
}
  const path = points.map((p, i) => (i === 0 ? 'M' : ' L') + x(i) + ',' + y(p.montoVenta)).join('')
  const profitPath = points
  .map(
    (p, i) =>
      (i === 0 ? 'M' : ' L') + x(i) + ',' + y(p.gananciaVenta),
  )
  .join('')

if (points.length === 0) {
  return <p>No hay ventas para mostrar en la gráfica.</p>
}

const hoveredPoint = hovered ? points[hovered.index] : undefined

const hoveredValue = hoveredPoint
  ? hovered?.series === 'ganancia'
    ? hoveredPoint.gananciaVenta
    : hoveredPoint.montoVenta
  : 0

const hoveredColor =
  hovered?.series === 'ganancia' ? '#15803d' : '#07558e'

const hoveredLabel =
  hovered?.series === 'ganancia' ? 'Ganancia' : 'Total de venta'
  return <div className="report-chart">
    <svg viewBox="0 0 960 340" role="img" aria-label="Importe y ganancia individual de cada venta">
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
      <rect x="86" y="26" width="848" height="258" fill="transparent"
        onPointerMove={event => hoverPoint(event.clientX, event.clientY, event.currentTarget.ownerSVGElement!)}
        onPointerDown={event => hoverPoint(event.clientX, event.clientY, event.currentTarget.ownerSVGElement!)}
        onPointerLeave={() => setHovered(null)} />
        {hovered && hoveredPoint && (
  <g pointerEvents="none" aria-hidden="true">
    <circle
      cx={x(hovered.index)}
      cy={y(hoveredValue)}
      r="6"
      fill={hoveredColor}
      stroke="white"
      strokeWidth="2"
    />

    <g
      transform={`translate(
        ${Math.min(810, Math.max(210, x(hovered.index)))},
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
        {hoveredLabel}: {money.format(hoveredValue)}
      </text>
    </g>
  </g>
)}
    </svg>
  </div>
}
export function ReportsPage() {
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
  if (!admin) return null
  return <section className="reports-panel" aria-labelledby="sales-report-title">
    <div className="reports-heading"><h2 id="sales-report-title">Importe de ventas por sesión de caja</h2><RefreshButton label="Actualizar reporte" loading={loading || sessionLoading} onClick={() => { setSessionPage(1); setRevision(v => v + 1) }} /></div>
    <div className="reports-filters"><label>Sesión de caja<select value={sessionId || ''} disabled={sessionLoading || !sessions.length} onChange={e => setSessionId(Number(e.target.value))}><option value="" disabled>Selecciona una sesión</option>{sessions.map(s => <option key={s.id_sesion_caja} value={s.id_sesion_caja}>{date.format(new Date(s.fecha_apertura))} · {s.fecha_cierre === null ? 'Abierta' : 'Cerrada'} · {s.usuario_apertura}</option>)}</select></label>
      {more && <button type="button" disabled={sessionLoading} onClick={() => setSessionPage(p => p + 1)}>Cargar sesiones anteriores</button>}
    </div>
    {sessionError && <p role="alert">{sessionError}</p>}
    {sessionLoading && <p role="status">Cargando sesiones…</p>}
    {!sessionLoading && !sessionError && !sessions.length && <p>No hay sesiones de caja registradas.</p>}
    {loading ? <p role="status">Cargando gráfica…</p> : error ? <div role="alert"><p>{error}</p><button type="button" onClick={() => setRevision(v => v + 1)}>Reintentar</button></div> : report && <>
      <div className="reports-totals"><div><span>Total de ventas</span><strong>{money.format(report.totalVentas)}</strong></div><div><span>Ventas registradas</span><strong>{report.cantidadVentas}</strong></div></div>
      
      {report.cantidadVentas === 0 && <p>No hay ventas registradas en esta sesión.</p>}
      <SalesChart key={sessionId} report={report} />
    </>}
  </section>
}
