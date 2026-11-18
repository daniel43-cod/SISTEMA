import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import type { SaleHistory } from './salesApi.ts'

export async function loadSaleSummary(token: string, page: number, pageSize: number, range?: { desde: string; hasta: string }, userId?: number, signal?: AbortSignal, filters?: { clientId?: number; search?: string; status?: 'pendientes' | 'pagadas' }) {
  const params = new URLSearchParams({ pagina: String(page), tamanoPagina: String(pageSize) })
  if (range) { params.set('desde', range.desde); params.set('hasta', range.hasta) }
  if (userId !== undefined) params.set('idUsuario', String(userId))
  if (filters?.clientId) params.set('idCliente', String(filters.clientId))
  if (filters?.search?.trim()) params.set('buscar', filters.search.trim())
  if (filters?.status) params.set('estadoPago', filters.status)
  const v = await requestJson('/Venta/resumen?' + params, { token, signal }) as Record<string, unknown> | null
  const finite = (value: unknown) => typeof value === 'number' && Number.isFinite(value)
  if (!v || v.pagina !== page || v.tamanoPagina !== pageSize || !Number.isSafeInteger(v.totalRegistros) || Number(v.totalRegistros) < 0 ||
    typeof v.hayMas !== 'boolean' || !finite(v.totalVentas) || !finite(v.gananciaTotal) || !Array.isArray(v.items) || v.items.length > pageSize ||
    !v.items.every(item => item && Number.isSafeInteger(item.id_ventas) && item.id_ventas > 0 && typeof item.cliente === 'string' &&
      typeof item.usuario === 'string' && finite(item.total) && finite(item.subtotal) && (item.descuento === null || finite(item.descuento)) &&
      finite(item.saldo_pendiente) && finite(item.ganancia_total) && typeof item.estado === 'string' && typeof item.fecha_venta === 'string'))
    throw new ApiError('El resumen de ventas no tiene el formato esperado.')
  const clients = v.clientesCajaActiva
  if (clients !== undefined && (!Array.isArray(clients) || !clients.every(c => c && Number.isSafeInteger(c.idCliente) && c.idCliente > 0 && typeof c.nombre === 'string')))
    throw new ApiError('El listado de clientes de caja no es válido.')
  for (const key of ['totalPorCobrar', 'cantidadPendientes', 'cantidadPagadas'])
    if (v[key] !== undefined && (!finite(v[key]) || Number(v[key]) < 0))
      throw new ApiError('El resumen de ventas no tiene el formato esperado.')
  return { totalRegistros: v.totalRegistros as number,
    totalPorCobrar: v.totalPorCobrar === undefined ? null : v.totalPorCobrar as number,
    cantidadPendientes: v.cantidadPendientes === undefined ? null : v.cantidadPendientes as number,
    cantidadPagadas: v.cantidadPagadas === undefined ? null : v.cantidadPagadas as number,
    clientesCajaActiva: (clients ?? []) as { idCliente: number; nombre: string }[], items: v.items as SaleHistory[], hayMas: v.hayMas, gananciaTotal: v.gananciaTotal as number, totalVentas: v.totalVentas as number }
}
