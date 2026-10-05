import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
export type ProductSuggestion = { idProducto: number; nombre: string }
export async function searchProducts(mode: 'nombre' | 'codigoBarra', text: string, token: string, signal?: AbortSignal): Promise<ProductSuggestion[]> {
  const value = text.trim()
  if (!value || value.length > (mode === 'nombre' ? 200 : 100) || (mode === 'nombre' && value.length < 3))
    throw new ApiError('Ingresa al menos tres caracteres del nombre o un código completo.', 400)
  const result = await requestJson('/Productos/buscar-administracion?' + new URLSearchParams({ [mode]: value }), { token, signal })
  if (!Array.isArray(result) || result.length > 10 || !result.every(item => item && Number.isSafeInteger(item.idProducto) && item.idProducto > 0 && typeof item.nombre === 'string') ||
    new Set(result.map(item => item.idProducto)).size !== result.length) throw new ApiError('Respuesta de búsqueda inválida.')
  return result as ProductSuggestion[]
}