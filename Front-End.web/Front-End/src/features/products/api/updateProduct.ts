import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
export type UpdateProductRequest = { codigo_barra: string; nombre: string; idMarca: number; stock_minimo: number }
const hasControl = (value: string) => Array.from(value).some(char => { const code = char.charCodeAt(0); return code < 32 || (code >= 127 && code <= 159) })
export async function updateProduct(id: number, data: UpdateProductRequest, token: string, signal?: AbortSignal) {
  const code = data.codigo_barra.trim(), name = data.nombre.trim()
  if (!Number.isSafeInteger(id) || id <= 0 || !code || code.length > 100 || /\s/.test(code) || hasControl(data.codigo_barra) ||
    !name || name.length > 200 || hasControl(data.nombre) || !Number.isSafeInteger(data.idMarca) || data.idMarca <= 0 ||
    !Number.isInteger(data.stock_minimo) || data.stock_minimo < 0 || data.stock_minimo > 2147483647)
    throw new ApiError('Revisa el nombre, código de barras, marca y existencia mínima.', 400)
  try {
    const result = await requestJson(`/Productos/${id}`, { method: 'PUT', token, signal,
      body: { codigo_barra: code, nombre: name, idMarca: data.idMarca, stock_minimo: data.stock_minimo } })
    if (!result || typeof result !== 'object' || !('idProducto' in result) || result.idProducto !== id)
      throw new ApiError('No se pudo confirmar la actualización. Actualiza el listado antes de reintentar.')
  } catch (error) {
    if (error instanceof ApiError && error.status === 409)
      throw new ApiError('Otro producto ya tiene ese nombre o código de barras.', 409)
    if (error instanceof ApiError && error.status === 404)
      throw new ApiError('El producto ya no está disponible.', 404)
    throw error
  }
}