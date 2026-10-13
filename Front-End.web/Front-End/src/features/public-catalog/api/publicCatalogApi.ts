import type { PublicBrand } from '../types/brand'
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

export async function listPublicBrands(idCategoria: number, signal?: AbortSignal): Promise<PublicBrand[]> {
  if (!Number.isSafeInteger(idCategoria) || idCategoria <= 0)
    throw new ApiError('La categoría seleccionada no es válida.', 400)
  let response: unknown
  try {
    response = await requestJson(`/CatalogoPublico/categorias/${idCategoria}/marcas`, { signal })
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 404)
      throw new ApiError('Esta categoría ya no está disponible. Vuelve a las categorías.', 404)
    throw failure
  }
  if (!Array.isArray(response)) throw new ApiError('El listado de marcas recibido no es válido.')
  const items = response.map((row: unknown): PublicBrand => {
    if (!row || typeof row !== 'object') throw new ApiError('La marca recibida no es válida.')
    const item = row as Record<string, unknown>
    if (typeof item.idMarca !== 'number' || !Number.isSafeInteger(item.idMarca) || item.idMarca <= 0 ||
        typeof item.nombre !== 'string' || !item.nombre.trim() ||
        (item.urlImagen !== null && typeof item.urlImagen !== 'string'))
      throw new ApiError('La marca recibida no es válida.')
    return { idMarca: item.idMarca, nombre: item.nombre, urlImagen: item.urlImagen }
  })
  if (new Set(items.map(item => item.idMarca)).size !== items.length)
    throw new ApiError('El listado de marcas recibido no es válido.')
  return items
}
