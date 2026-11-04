import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'
export type ReportSession = { id_sesion_caja: number; fecha_apertura: string; fecha_cierre: string | null; usuario_apertura: string }
//export type SalesPoint = { idVenta: number | null; fecha: string; montoVenta: number; acumulado: number }
export type SalesPoint = {
idVenta: number | null
  fecha: string
  montoVenta: number
  gananciaVenta: number
  acumulado: number
}
export type SalesReport = { idSesionCaja: number; apertura: string; cierre: string | null; hasta: string; totalVentas: number; cantidadVentas: number; puntos: SalesPoint[] }
const record = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null
const date = (v: unknown): v is string => typeof v === 'string' && Number.isFinite(Date.parse(v))
const amount = (v: unknown) => typeof v === 'number' && Number.isFinite(v) && v >= 0
const count = (v: unknown) => Number.isSafeInteger(v) && Number(v) >= 0
export async function listReportSessions(token: string, page: number, signal?: AbortSignal): Promise<ReportSession[]> {
  const result = await requestJson('/Caja/ListarSesionesCaja?pagina=' + page + '&tamanoPagina=50', { token, signal })
  if (!Array.isArray(result) || result.length > 50 || !result.every(s => record(s) && Number.isSafeInteger(s.id_sesion_caja) && Number(s.id_sesion_caja) > 0 && date(s.fecha_apertura) && (s.fecha_cierre === null || date(s.fecha_cierre)) && typeof s.usuario_apertura === 'string'))
    throw new ApiError('No se pudo interpretar el listado de sesiones.')
  return result as ReportSession[]
}
export async function getSalesReport(token: string, sessionId: number, signal?: AbortSignal): Promise<SalesReport> {
  const v = await requestJson('/reportes/ventas-acumuladas?idSesionCaja=' + sessionId, { token, signal })
  if (!record(v) || v.idSesionCaja !== sessionId || !date(v.apertura)
     || !date(v.hasta) || (v.cierre !== null && !date(v.cierre))
     || !amount(v.totalVentas) || !count(v.cantidadVentas) 
     || !Array.isArray(v.puntos) || v.puntos.length !== Number(v.cantidadVentas) + 1 
     || !v.puntos.every((p, i) => record(p) && (i === 0 ? p.idVenta === null : Number.isSafeInteger(p.idVenta)
      && Number(p.idVenta) > 0) && date(p.fecha) 
      && amount(p.montoVenta) &&
typeof p.gananciaVenta === 'number' &&
Number.isFinite(p.gananciaVenta) &&
amount(p.acumulado)))
    throw new ApiError('El reporte de ventas no tiene el formato esperado.')
  let accumulated = 0
  let previousDate = Date.parse(v.apertura)
  for (const [i, p] of (v.puntos as SalesPoint[]).entries()) {
    accumulated += p.montoVenta
    if (Date.parse(p.fecha) < previousDate || Date.parse(p.fecha) > Date.parse(v.hasta) || Math.abs(p.acumulado - accumulated) > 0.005 || (i === 0 && (p.fecha !== v.apertura || p.montoVenta !== 0)))
      throw new ApiError('Los puntos del reporte no son consistentes.')
    previousDate = Date.parse(p.fecha)
  }
  if (Math.abs(accumulated - Number(v.totalVentas)) > 0.005) throw new ApiError('El total del reporte no es consistente.')
  return v as SalesReport
}
