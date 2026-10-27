import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'

export type OrderSummary = { idPedido: number; fechaUtc: string; nombreCliente: string; telefono: string; direccion: string;
  referencias: string | null; observacion: string | null; zonaEntrega: string; total: number; estado: string }
export type OrderLine = { idProductoPresentacion: number; nombreProducto: string; nombrePresentacion: string;
  cantidad: number; precioUnitario: number; subtotal: number }
export type OrderDetail = OrderSummary & { detalles: OrderLine[] }
export type OrderPage = { pagina: number; tamanoPagina: number; hayMas: boolean; pedidos: OrderSummary[] }
const record = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null
const id = (v: unknown) => Number.isSafeInteger(v) && Number(v) > 0
const amount = (v: unknown) => typeof v === 'number' && Number.isFinite(v) && v >= 0
const optionalText = (v: unknown) => v === null || typeof v === 'string'
function summary(v: unknown): v is OrderSummary {
  return record(v) && id(v.idPedido) && typeof v.fechaUtc === 'string' && !Number.isNaN(Date.parse(v.fechaUtc)) &&
    typeof v.nombreCliente === 'string' && typeof v.telefono === 'string' && typeof v.direccion === 'string' &&
    optionalText(v.referencias) && optionalText(v.observacion) && typeof v.zonaEntrega === 'string' && amount(v.total) && typeof v.estado === 'string'
}
export async function listPendingOrders(token: string, page: number, signal?: AbortSignal): Promise<OrderPage> {
  const v = await requestJson('/pedidos-catalogo/pendientes?pagina=' + page, { token, signal })
  if (!record(v) || v.pagina !== page || v.tamanoPagina !== 50 || typeof v.hayMas !== 'boolean' ||
    !Array.isArray(v.pedidos) || v.pedidos.length > 50 || !v.pedidos.every(summary))
    throw new ApiError('El listado de pedidos no tiene el formato esperado.')
  return v as OrderPage
}
export async function getOrderDetail(token: string, orderId: number, signal?: AbortSignal): Promise<OrderDetail> {
  const v = await requestJson('/pedidos-catalogo/' + orderId + '/detalle', { token, signal })
  const details = record(v) ? v.detalles : undefined
  if (!summary(v) || v.idPedido !== orderId || !Array.isArray(details) || !details.every(d => record(d) &&
    id(d.idProductoPresentacion) && typeof d.nombreProducto === 'string' && typeof d.nombrePresentacion === 'string' &&
    id(d.cantidad) && amount(d.precioUnitario) && amount(d.subtotal)))
    throw new ApiError('El detalle del pedido no tiene el formato esperado.')
  return v as OrderDetail
}
