import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import type { CreateBrandRequest, BrandResponse } from '../types/brand.ts'

export async function createBrand(data: CreateBrandRequest, token: string, signal?: AbortSignal): Promise<BrandResponse> {
  if (!data.nombre.trim() || data.nombre.trim().length > 100 ||
      !Number.isSafeInteger(data.idCategoria) || data.idCategoria <= 0)
    throw new ApiError('Ingresa un nombre de hasta 100 caracteres y selecciona una categoría.', 400)
  try {
    const result = await requestJson('/Marcas', {
      method: 'POST', body: { nombre: data.nombre.trim(), idCategoria: data.idCategoria }, token, signal,
    })
    const row = result as Partial<BrandResponse> | null
    if (!row || !Number.isSafeInteger(row.idMarca) || row.idMarca! <= 0 ||
        typeof row.nombre !== 'string' || !row.nombre.trim() ||
        row.idCategoria !== data.idCategoria || typeof row.estado !== 'boolean')
      throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
    return row as BrandResponse
  } catch (error) {
    if (error instanceof ApiError && error.status === 409)
      throw new ApiError('Ya existe una marca con ese nombre, incluso si está inactiva.', 409)
    if (error instanceof ApiError && error.status === 0)
      throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
    throw error
  }
}
