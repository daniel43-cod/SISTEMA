import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type FelMessage = { fuente: string | null; mensaje: string | null }
export type FelDocument = {
  id_documento_fel: number; id_venta: number; ambiente: string; estado: string
  uuid: string | null; serie: string | null; numero: string | null; pdf: string | null
  descripcion?: string | null; errores: FelMessage[]; alertas: FelMessage[]
}
export type FelAddress = {
  direccion: string; codigo_postal: string; municipio: string; departamento: string
  pais: string; correo_receptor?: string; confirmar_bienes_gravados: boolean
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
