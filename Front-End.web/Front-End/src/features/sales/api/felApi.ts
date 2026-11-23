import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type FelMessage = { fuente: string | null; mensaje: string | null }
export type FelDocument = {
  id_documento_fel: number; id_venta: number; ambiente: string; estado: string
  uuid: string | null; serie: string | null; numero: string | null; pdf: string | null
  descripcion?: string | null; puede_recuperar?: boolean; procesamiento_vence?: string | null; recuperaciones?: number; errores: FelMessage[]; alertas: FelMessage[]
}
export type FelAddress = {
  tipo_identificacion_receptor?: 'CF' | 'NIT' | 'CUI'; identificacion_receptor?: string
  direccion?: string; codigo_postal?: string; municipio?: string; departamento?: string
  pais?: string; correo_receptor?: string; confirmar_bienes_gravados: boolean
}
const record = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null
export function parseFelDocument(value: unknown, saleId: number): FelDocument {
  if (!record(value) || value.id_venta !== saleId || !Number.isSafeInteger(value.id_documento_fel) ||
      !['PENDIENTE', 'EN_PROCESO', 'POR_CONFIRMAR', 'RECHAZADA', 'CERTIFICADA'].includes(String(value.estado)) ||
      typeof value.ambiente !== 'string' || !Array.isArray(value.errores) || !Array.isArray(value.alertas) ||
      ![...value.errores, ...value.alertas].every(m => record(m) && (m.mensaje === null || typeof m.mensaje === 'string')))
    throw new ApiError('No se pudo interpretar el estado de certificación. Consulta el estado antes de volver a enviar.')
  for (const field of ['uuid', 'serie', 'numero', 'pdf'] as const)
    if (value[field] !== null && typeof value[field] !== 'string') throw new ApiError('Respuesta FEL inválida.')
  if (typeof value.pdf === 'string') {
    const url = new URL(value.pdf)
    if (url.origin !== 'https://report.feel.com.gt' || url.pathname !== '/ingfacereport/ingfacereport_documento')
      throw new ApiError('El enlace de factura no pertenece al servicio de INFILE.')
  }
  return value as FelDocument
}
export async function loadFel(saleId: number, token: string, signal?: AbortSignal) {
  const result = await requestJson(`/Venta/${saleId}/fel`, { token, signal })
  if (!record(result) || !('documento' in result)) throw new ApiError('Respuesta de consulta FEL inválida.')
  return result.documento === null ? null : parseFelDocument(result.documento, saleId)
}
export async function certifySale(saleId: number, body: FelAddress, token: string) {
  const result = await requestJson(`/Venta/${saleId}/certificar`, {
    method: 'POST', body, token, validationMessages: true, timeoutMs: 90000,
  })
  return parseFelDocument(result, saleId)
}

export async function loadFelPreparation(saleId: number, token: string, signal?: AbortSignal): Promise<{ total: number }> {
  const result = await requestJson(`/Venta/${saleId}/fel/preparacion`, { token, signal })
  if (!record(result) || typeof result.total !== 'number' || !Number.isFinite(result.total) || result.total <= 0)
    throw new ApiError('No se pudo obtener el total de la venta para certificar.')
  return { total: result.total }
}

export async function recoverFel(saleId: number, token: string) {
  const result = await requestJson(`/Venta/${saleId}/fel/recuperar`, {
    method: 'POST', token, timeoutMs: 90000,
  })
  return parseFelDocument(result, saleId)
}

export type FelListStatus = { id_venta: number; estado: 'SIN_CERTIFICAR' | 'PENDIENTE' | 'CERTIFICADA' | 'RECHAZADA' | 'EN_PROCESO' | 'POR_CONFIRMAR'; ambiente: 'PRUEBAS' | 'PRODUCCION' }
export async function loadFelStatuses(ids: number[], token: string, signal?: AbortSignal): Promise<FelListStatus[]> {
  if (ids.length > 100 || ids.some(id => !Number.isSafeInteger(id) || id < 1) || new Set(ids).size !== ids.length)
    throw new ApiError('La consulta de estados FEL no es válida.')
  if (!ids.length) return []
  const result = await requestJson('/Fel/ventas/estados', { method: 'POST', body: ids, token, signal })
  if (!Array.isArray(result) || result.length !== ids.length || new Set(result.map(r => record(r) ? r.id_venta : null)).size !== ids.length ||
      !result.every(r => record(r) && ids.includes(r.id_venta as number) && ['SIN_CERTIFICAR','PENDIENTE','CERTIFICADA','RECHAZADA','EN_PROCESO','POR_CONFIRMAR'].includes(String(r.estado)) && ['PRUEBAS','PRODUCCION'].includes(String(r.ambiente))))
    throw new ApiError('No se pudieron interpretar los estados FEL. Vuelve a consultarlos.')
  return result as FelListStatus[]
}
