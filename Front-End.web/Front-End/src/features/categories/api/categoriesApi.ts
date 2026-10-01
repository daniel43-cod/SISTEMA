import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import { parseCategory, validateCategoryName } from '../schemas/categorySchema.ts'
import type { CreateCategoryRequest } from '../types/category.ts'

export async function createCategory(data: CreateCategoryRequest, token: string, signal?: AbortSignal) {
  const validation = validateCategoryName(data.nombre)
  if (validation) throw new ApiError(validation, 400)
  const url = data.urlImagen?.trim()
  if (url && data.imagen) throw new ApiError('Elige un enlace o un archivo.', 400)
  if (url) {
    try {
      const parsed = new URL(url)
      if (url.length > 2048 || parsed.protocol !== 'https:' || parsed.username || parsed.password) throw new Error()
    } catch { throw new ApiError('Ingresa una URL HTTPS válida.', 400) }
  }
  let body: unknown = { nombre: data.nombre.trim(), ...(url ? { urlImagen: url } : {}) }
  if (data.imagen) {
    if (data.imagen.size === 0 || data.imagen.size > 5 * 1024 * 1024 ||
        !['image/jpeg', 'image/png', 'image/webp'].includes(data.imagen.type))
      throw new ApiError('Selecciona una imagen JPEG, PNG o WebP de hasta 5 MB.', 400)
    const form = new FormData()
    form.append('Nombre', data.nombre.trim())
    form.append('Imagen', data.imagen)
    body = form
  }
  try {
    const response = await requestJson(data.imagen ? '/Categoria/con-imagen' : '/Categoria', { method: 'POST', body, token, signal })
    try { return parseCategory(response) }
    catch { throw new ApiError('No se pudo confirmar el registro.') }
  } catch (error) {
    if (error instanceof ApiError && error.status === 409)
      throw new ApiError('Ya existe una categoría con ese nombre, incluso si está inactiva.', 409)
    if (error instanceof ApiError && error.status === 413)
      throw new ApiError('La imagen supera el tamaño permitido.', 413)
    if (error instanceof ApiError && error.status === 0)
      throw new ApiError('No se pudo confirmar el registro. Revisa la conexión y verifica si se guardó antes de reintentar.')
    throw error
  }
}

