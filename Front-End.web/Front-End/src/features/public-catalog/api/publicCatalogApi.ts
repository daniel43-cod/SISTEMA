import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'
import type { PublicCategory } from '../types/category'

export async function listPublicCategories(signal?: AbortSignal): Promise<PublicCategory[]> {
  const response = await requestJson('/CatalogoPublico/categorias', { signal })
  if (!Array.isArray(response)) throw new ApiError('El listado de categorías recibido no es válido.')
  const items = response.map((row: unknown): PublicCategory => {
    if (!row || typeof row !== 'object') throw new ApiError('La categoría recibida no es válida.')
    const item = row as Record<string, unknown>
    if (typeof item.idCategoria !== 'number' || !Number.isSafeInteger(item.idCategoria) || item.idCategoria <= 0 ||
        typeof item.nombre !== 'string' || !item.nombre.trim() ||
        (item.urlImagen !== null && typeof item.urlImagen !== 'string'))
      throw new ApiError('La categoría recibida no es válida.')
    return { idCategoria: item.idCategoria, nombre: item.nombre, urlImagen: item.urlImagen }
  })
  if (new Set(items.map(item => item.idCategoria)).size !== items.length)
    throw new ApiError('El listado de categorías recibido no es válido.')
  return items
}
