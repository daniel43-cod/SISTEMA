import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import type { CreateBrandRequest, BrandResponse } from '../types/brand.ts'

export function createBrand(data: CreateBrandRequest, token: string, signal?: AbortSignal) {
  return saveBrand(data, token, signal)
}
// Usa el ID de la marca seleccionada para actualizar sin crear otro registro.
export function updateBrand(id: number, data: CreateBrandRequest, token: string, signal?: AbortSignal) {
  if (!Number.isSafeInteger(id) || id <= 0) return Promise.reject(new ApiError('ID inválido.', 400))
  return saveBrand(data, token, signal, id)
}
async function saveBrand(data: CreateBrandRequest, token: string, signal?: AbortSignal, id?: number): Promise<BrandResponse> {
  if (!data.nombre.trim() || data.nombre.trim().length > 100 ||
      !Number.isSafeInteger(data.idCategoria) || data.idCategoria <= 0)
    throw new ApiError('Ingresa un nombre de hasta 100 caracteres y selecciona una categoría.', 400)
  const urlImagen = data.urlImagen?.trim() || undefined
  if (urlImagen) {
    let url: URL
    try { url = new URL(urlImagen) } catch { throw new ApiError('Ingresa un enlace HTTPS válido.', 400) }
    if (url.protocol !== 'https:' || url.username || url.password || url.href.length > 2048)
      throw new ApiError('Ingresa un enlace HTTPS de hasta 2048 caracteres.', 400)
  }
  if (data.quitarImagen && (id === undefined || data.imagen || urlImagen))
    throw new ApiError('No puedes quitar y reemplazar la imagen al mismo tiempo.', 400)
  if (data.imagen && urlImagen) throw new ApiError('Elige un archivo o un enlace, no ambos.', 400)
  if (data.imagen && (!['image/jpeg', 'image/png', 'image/webp'].includes(data.imagen.type) ||
      data.imagen.size === 0 || data.imagen.size > 5 * 1024 * 1024))
    throw new ApiError('Selecciona una imagen JPEG, PNG o WebP de hasta 5 MB.', 400)
  let body: FormData | { nombre: string; idCategoria: number; urlImagen?: string; quitarImagen?: boolean } = {
    nombre: data.nombre.trim(), idCategoria: data.idCategoria, ...(urlImagen ? { urlImagen } : {}), ...(data.quitarImagen ? { quitarImagen: true } : {}),
  }
  if (data.imagen) {
    body = new FormData()
    body.append('Nombre', data.nombre.trim())
    body.append('IdCategoria', String(data.idCategoria))
    body.append('Imagen', data.imagen)
  }
  try {
    const result = await requestJson(id ? '/Marcas/' + id + (data.imagen ? '/con-imagen' : '') : data.imagen ? '/Marcas/con-imagen' : '/Marcas', {
      method: id ? 'PUT' : 'POST', body, token, signal, validationMessages: true,
    })
    const row = result as Partial<BrandResponse> | null
    if (!row || !Number.isSafeInteger(row.idMarca) || row.idMarca! <= 0 ||
        typeof row.nombre !== 'string' || !row.nombre.trim() ||
        (id !== undefined && row.idMarca !== id) || row.idCategoria !== data.idCategoria || typeof row.estado !== 'boolean')
      throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
    return row as BrandResponse
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) throw new ApiError('La marca ya no existe. Actualiza el listado.', 404)
    if (error instanceof ApiError && error.status === 409)
      throw new ApiError('Ya existe una marca con ese nombre, incluso si está inactiva.', 409)
    if (error instanceof ApiError && error.status === 0)
      throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
    throw error
  }
}

// Consulta las marcas activas con el token de sesión y valida la respuesta antes de mostrarla.
export async function listBrands(token: string, signal?: AbortSignal): Promise<BrandResponse[]> {
  const result = await requestJson('/Marcas', { token, signal })
  if (!Array.isArray(result)) throw new ApiError('El listado de marcas recibido no es válido.')
  const ids = new Set<number>()
  return result.map(value => {
    const row = value as Partial<BrandResponse> | null
    if (!row || !Number.isSafeInteger(row.idMarca) || row.idMarca! <= 0 ||
        !Number.isSafeInteger(row.idCategoria) || row.idCategoria! <= 0 ||
        typeof row.nombre !== 'string' || !row.nombre.trim() || row.estado !== true ||
        (row.nombreCategoria != null && typeof row.nombreCategoria !== 'string') ||
        (row.urlImagen != null && typeof row.urlImagen !== 'string') || ids.has(row.idMarca!))
      throw new ApiError('El listado de marcas recibido no es válido.')
    ids.add(row.idMarca!)
    return { ...(row.urlImagen != null ? { urlImagen: row.urlImagen } : {}), ...(row.nombreCategoria != null ? { nombreCategoria: row.nombreCategoria } : {}), idMarca: row.idMarca!, nombre: row.nombre, idCategoria: row.idCategoria!, estado: row.estado }
  })
}




