import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'

export type DeliveryZone = { idZonaEntrega: number; nombre: string; minimoCompra: number }
export type PublicOrder = { idOperacion: string; nombreCliente: string; telefono: string; direccion: string;
  referencias: string; observacion: string; idZonaEntrega: number;
  detalles: { idProductoPresentacion: number; cantidad: number }[] }
const record = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null
export async function loadDeliveryZones(signal?: AbortSignal): Promise<DeliveryZone[]> {
  const result = await requestJson('/pedidos-catalogo/zonas', { signal })
  if (!Array.isArray(result) || !result.every(v => record(v) && Number.isSafeInteger(v.idZonaEntrega) && Number(v.idZonaEntrega) > 0 &&
    typeof v.nombre === 'string' && typeof v.minimoCompra === 'number' && Number.isFinite(v.minimoCompra) && v.minimoCompra > 0))
    throw new ApiError('No se pudieron interpretar las zonas de entrega.')
  return result as DeliveryZone[]
}
export async function submitPublicOrder(body: PublicOrder) {
  // No reintentar automáticamente: se conserva la misma operación ante una respuesta incierta.
  const result = await requestJson('/pedidos-catalogo', { method: 'POST', body, validationMessages: true })
  if (!record(result) || result.idOperacion !== body.idOperacion || typeof result.total !== 'number' ||
    !Number.isFinite(result.total) || result.total <= 0 || typeof result.estado !== 'string')
    throw new ApiError('No se pudo confirmar la respuesta del pedido.', 200)
  return { total: result.total, estado: result.estado }
}
