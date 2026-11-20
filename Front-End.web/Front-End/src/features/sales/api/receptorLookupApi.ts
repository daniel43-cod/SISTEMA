import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export async function lookupReceiver(tipo: 'NIT' | 'CUI', identificacion: string, token: string, signal?: AbortSignal): Promise<string> {
  const value = await requestJson('/Fel/receptor/consultar', {
    method: 'POST', body: { tipo, identificacion }, token, signal, validationMessages: true, timeoutMs: 65000,
  })
  if (!value || typeof value !== 'object') throw new ApiError('La consulta del receptor devolvió un formato inválido.')
  const result = value as Record<string, unknown>
  if (result.tipo !== tipo || result.identificacion !== identificacion || typeof result.resultado !== 'boolean')
    throw new ApiError('La respuesta no corresponde al documento consultado.')
  if (!result.resultado) throw new Error(tipo === 'NIT' ? 'INFILE no encontró una razón social válida. Revisa el NIT.' : 'INFILE no encontró un CUI activo. Revisa el documento.')
  if (typeof result.nombre !== 'string' || !result.nombre.trim() || result.nombre.length > 250)
    throw new ApiError('INFILE no devolvió un nombre válido.')
  return result.nombre.trim()
}
