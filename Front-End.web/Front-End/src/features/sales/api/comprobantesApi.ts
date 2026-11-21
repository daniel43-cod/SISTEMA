import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type ReceiptOptions = { telefono: string | null; empresas: { id: number; nombre: string }[] }
export type ReceiptResult = { idEnvio: string; huboFallo: boolean; mensaje: string; resultado: { idVenta: number; estado: string } }
const record = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null
const id = (v: unknown): v is number => Number.isSafeInteger(v) && Number(v) > 0
export function validReceiptPhone(phone: string): boolean {
  const digits = phone.trim().replace(/[\s()-]/g, '').replace(/^\+/, '')
  return /^\d{8}$/.test(digits) || /^[1-9]\d{8,14}$/.test(digits)
}
function result(v: unknown, saleId?: number): ReceiptResult {
  if (!record(v) || typeof v.idEnvio !== 'string' || !/^[0-9a-f-]{36}$/i.test(v.idEnvio) || typeof v.huboFallo !== 'boolean' ||
      typeof v.mensaje !== 'string' || v.mensaje.length > 500 || !record(v.resultado) || !id(v.resultado.idVenta) ||
      (saleId !== undefined && v.resultado.idVenta !== saleId) || typeof v.resultado.estado !== 'string')
    throw new ApiError('No se pudo confirmar el envío del comprobante.')
  return v as ReceiptResult
}
export async function receiptOptions(saleId: number, token: string, signal?: AbortSignal): Promise<ReceiptOptions> {
  const v = await requestJson(`/comprobantes/ventas/${saleId}/opciones-whatsapp`, { token, signal })
  if (!record(v) || (v.telefono !== null && typeof v.telefono !== 'string') || !Array.isArray(v.empresas) ||
      !v.empresas.every(e => record(e) && id(e.id) && typeof e.nombre === 'string')) throw new ApiError('No se pudieron cargar los datos del comprobante.')
  return v as ReceiptOptions
}
export async function sendReceipt(saleId: number, companyId: number, registeredPhone: string | null,
  enteredPhone: string, mode: 'registro' | 'historial' | 'pagos', token: string, signal?: AbortSignal): Promise<ReceiptResult> {
  if (!id(saleId) || !id(companyId)) throw new Error('Selecciona la empresa del comprobante.')
  const saved = !!registeredPhone?.trim()
  const phone = saved ? registeredPhone! : enteredPhone.trim()
  if (!phone.trim()) throw new Error('Ingresa el número de teléfono primero.')
  if (!validReceiptPhone(phone)) throw new Error('Ingresa un número de teléfono válido.')
  const suffix = saved ? '' : `/${mode}`
  const route = mode === 'pagos' ? `/comprobantes/ventas/${saleId}/pagos/whatsapp` : `/comprobantes/ventas/${saleId}/whatsapp${suffix}`
  return result(await requestJson(`${route}?idEmpresa=${companyId}`, {
    method: 'POST', token, signal, validationMessages: true, timeoutMs: 180000,
    ...(saved ? {} : { body: { telefono: enteredPhone.trim() } }),
  }), saleId)
}
export async function receiptStatus(sendId: string, token: string, signal?: AbortSignal): Promise<ReceiptResult> {
  return result(await requestJson(`/comprobantes/whatsapp/${encodeURIComponent(sendId)}`, { token, signal }))
}
