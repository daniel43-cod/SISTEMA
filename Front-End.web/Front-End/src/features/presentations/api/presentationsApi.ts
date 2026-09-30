import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import { parsePresentation } from '../schemas/presentationSchema.ts'
import type { CreatePresentationRequest, UpdatePresentationRequest } from '../types/presentation.ts'

export async function createPresentation(data: CreatePresentationRequest, token: string, signal?: AbortSignal) {
  try {
    const response = await requestJson('/Presentaciones', {
      method: 'POST', body: { descripcion: data.descripcion.trim() }, token, signal,
    })
    try { return parsePresentation(response) }
    catch { throw new ApiError('El servidor respondió, pero no se pudo confirmar el registro. Verifica antes de reintentar.') }
  } catch (error) {
    if (error instanceof ApiError && error.status === 409)
      throw new ApiError('Ya existe una presentación con esa descripción, incluso si está inactiva.', 409)
    if (error instanceof ApiError && error.status === 0)
      throw new ApiError('No se pudo confirmar el registro. Revisa la conexión y verifica si se guardó antes de reintentar.')
    throw error
  }
}
export async function listPresentations(token: string, signal?: AbortSignal) {
  const data = await requestJson('/Presentaciones', { token, signal })
  try {
    if (!Array.isArray(data)) throw new Error()
    const items = data.map(parsePresentation)
    if (new Set(items.map(item => item.idPresentacion)).size !== items.length) throw new Error()
    return items
  } catch { throw new ApiError('El servidor devolvió un listado de presentaciones inválido.') }
}


export async function updatePresentation(id: number, data: UpdatePresentationRequest, token: string, signal?: AbortSignal) {
  if (!Number.isSafeInteger(id) || id <= 0) throw new ApiError('El ID de la presentación no es válido.', 400)
  try {
    const response = await requestJson('/Presentaciones/' + id, {
      method: 'PUT', body: { descripcion: data.descripcion.trim() }, token, signal,
    })
    try {
      const updated = parsePresentation(response)
      if (updated.idPresentacion !== id) throw new Error()
      return updated
    } catch { throw new ApiError('No se pudo confirmar la actualización. Actualiza el listado antes de reintentar.') }
  } catch (error) {
    if (error instanceof ApiError && error.status === 409)
      throw new ApiError('Ya existe otra presentación con esa descripción.', 409)
    if (error instanceof ApiError && error.status === 404)
      throw new ApiError('La presentación ya no existe. Actualiza el listado.', 404)
    if (error instanceof ApiError && error.status === 0)
      throw new ApiError('No se pudo confirmar la actualización. Actualiza el listado antes de reintentar.')
    throw error
  }
}