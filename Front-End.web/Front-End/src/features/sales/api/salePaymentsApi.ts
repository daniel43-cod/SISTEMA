import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type SalePayment = { idPago: number; idVenta: number; fechaPago: string; monto: number; metodoPago: string; observacion: string | null; usuario: string }
export async function loadSalePayments(saleId: number, token: string, signal?: AbortSignal): Promise<SalePayment[]> {
  if (!Number.isSafeInteger(saleId) || saleId <= 0) throw new ApiError('Venta inválida.', 400)
  const result = await requestJson('/Pagos/venta/' + saleId, { token, signal })
  if (!Array.isArray(result) || !result.every(v => v && typeof v === 'object' && Number.isSafeInteger(v.idPago) && v.idPago > 0 &&
    v.idVenta === saleId && typeof v.fechaPago === 'string' && !Number.isNaN(Date.parse(v.fechaPago)) &&
    typeof v.monto === 'number' && Number.isFinite(v.monto) && v.monto > 0 && typeof v.metodoPago === 'string' &&
    typeof v.usuario === 'string' && (v.observacion === null || typeof v.observacion === 'string')) ||
    new Set(result.map(v => v.idPago)).size !== result.length)
    throw new ApiError('El historial de pagos no tiene el formato esperado.')
  return result.map(v => ({ idPago: v.idPago, idVenta: v.idVenta, fechaPago: v.fechaPago, monto: v.monto,
    metodoPago: v.metodoPago, observacion: v.observacion, usuario: v.usuario }))
}
