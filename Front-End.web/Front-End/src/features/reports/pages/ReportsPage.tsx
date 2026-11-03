import { useEffect, useId, useState } from 'react'
import { useAuth } from '../../auth'
import { ApiError } from '../../../shared/api/ApiError'
import { RefreshButton } from '../../../shared/ui/RefreshButton'
import { getSalesReport, listReportSessions } from '../api/reportsApi'
import type { ReportSession, SalesReport } from '../api/reportsApi'
import './ReportsPage.css'
const money = new Intl.NumberFormat('es-GT', { style: 'currency', currency: 'GTQ' })
const date = new Intl.DateTimeFormat('es-GT', { dateStyle: 'short', timeStyle: 'short' })
function SalesChart({ report }: { report: SalesReport }) {
  const titleId = useId()
  const [active, setActive] = useState(0)
  const [hovered, setHovered] = useState<number | null>(null)
  const points = report.puntos.filter(p => p.idVenta !== null)
  const max = points.reduce((maximum, p) => Math.max(maximum, p.montoVenta), 1)
  const beginning = Date.parse(report.apertura)
  const ending = Math.max(beginning + 1, Date.parse(report.hasta))
  const xTime = (time: number) => 100 + (time - beginning) / (ending - beginning) * 820
  const x = (i: number) => xTime(Date.parse(points[i]!.fecha))
  const y = (value: number) => 270 - value / max * 220
  const index = Math.min(active, points.length - 1)
  const selected = points[index]!
  const path = points.map((p, i) => (i === 0 ? 'M' : ' L') + x(i) + ',' + y(p.montoVenta)).join('')
  function hoverPoint(clientX: number, clientY: number, svg: SVGSVGElement) {
    const bounds = svg.getBoundingClientRect()
    const sx = (clientX - bounds.left) / bounds.width * 960
    const sy = (clientY - bounds.top) / bounds.height * 340
    let nearest: number | null = null
    let distance = 14 * 14
    points.forEach((p, i) => {
      const next = (x(i) - sx) ** 2 + (y(p.montoVenta) - sy) ** 2
      if (next <= distance) { nearest = i; distance = next }
    })
    setHovered(nearest)
    if (nearest !== null) setActive(nearest)
  }
  if (!selected) return <p>No hay ventas para mostrar en la gráfica.</p>
  return <div className="report-chart">
    <svg viewBox="0 0 960 340" role="img" aria-labelledby={titleId}>
      <title id={titleId}>Importe de cada venta desde {date.format(new Date(report.apertura))}. Total: {money.format(report.totalVentas)}.</title>
      {[0, 1, 2, 3, 4].map(i => <g key={i}><line x1="100" x2="920" y1={y(max * i / 4)} y2={y(max * i / 4)} stroke="#dce5ee" /><text x="90" y={y(max * i / 4) + 4} textAnchor="end">{money.format(max * i / 4)}</text></g>)}
      <path d={path} fill="none" stroke="#0876bc" strokeWidth="3" strokeLinejoin="round" />
      {points.length <= 60 && points.map((p, i) => <circle key={p.idVenta ?? 'opening'} cx={x(i)} cy={y(p.montoVenta)} r="3" fill="#0876bc" />)}
      {[0, 1, 2, 3, 4].map(i => <text key={i} x={100 + i * 205} y="302" textAnchor="middle">{date.format(new Date(beginning + (ending - beginning) * i / 4))}</text>)}
      <circle cx={x(index)} cy={y(selected.montoVenta)} r="6" fill="#07558e" stroke="white" strokeWidth="2" />
      <rect x="86" y="26" width="848" height="258" fill="transparent"
        onPointerMove={event => hoverPoint(event.clientX, event.clientY, event.currentTarget.ownerSVGElement!)}
        onPointerDown={event => hoverPoint(event.clientX, event.clientY, event.currentTarget.ownerSVGElement!)}
        onPointerLeave={() => setHovered(null)} />
      {hovered !== null && <g pointerEvents="none" aria-hidden="true" transform={'translate(' + Math.min(852, Math.max(168, x(hovered))) + ',' + Math.max(30, y(points[hovered]!.montoVenta) - 24) + ')'}>
        <rect x="-68" y="-22" width="136" height="32" rx="6" fill="#07558e" />
        <text textAnchor="middle" y="-2" style={{ fill: '#fff', fontWeight: 600 }}>{money.format(points[hovered]!.montoVenta)}</text>
      </g>}
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
