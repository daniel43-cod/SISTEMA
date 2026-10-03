import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import { validateProduct } from '../schemas/productSchema.ts'
import type { CreateProductRequest } from '../types/product.ts'

export async function createProduct(data: CreateProductRequest, token: string, signal?: AbortSignal) {
  const error = validateProduct(data)
  if (error) throw new ApiError(error, 400)
  const payload = {
    codigo_barra: data.codigo_barra.trim(), nombre: data.nombre.trim(), idMarca: data.idMarca,
    stock_minimo: data.stock_minimo, presentaciones: data.presentaciones,
    ...(data.urlImagen?.trim() ? { urlImagen: data.urlImagen.trim() } : {}),
  }
  let body: unknown = payload
  if (data.imagen) {
    const form = new FormData()
    form.append('codigo_barra', payload.codigo_barra)
    form.append('nombre', payload.nombre)
    form.append('IdMarca', String(payload.idMarca))
    form.append('stock_minimo', String(payload.stock_minimo))
    form.append('Imagen', data.imagen)
    // ASP.NET enlaza cada elemento del formulario mediante su índice.
    data.presentaciones.forEach((row, index) => {
      Object.entries(row).forEach(([key, value]) => form.append('presentaciones[' + index + '].' + key, String(value)))
    })
    body = form
  }
  try {
    const response = await requestJson('/Productos/crear' + (data.imagen ? '/con-imagen' : ''), {
      method: 'POST', body, token, signal,
    })
    const result = response as { id_producto?: unknown } | null
    if (!result || !Number.isSafeInteger(result.id_producto) || Number(result.id_producto) <= 0)
      throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
    return Number(result.id_producto)
  } catch (failure) {
    if (failure instanceof ApiError && failure.status === 409)
      throw new ApiError('Ya existe un producto con ese código de barras.', 409)
    if (failure instanceof ApiError && failure.status === 413)
      throw new ApiError('La imagen supera el tamaño permitido.', 413)
    if (failure instanceof ApiError && failure.status === 0)
      throw new ApiError('No se pudo confirmar el registro. Verifica si se guardó antes de reintentar.')
    throw failure
  }
}
