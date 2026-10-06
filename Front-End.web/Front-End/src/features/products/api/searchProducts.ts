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
export async function loadProductNames(token: string, signal?: AbortSignal): Promise<ProductSuggestion[]> {
  const result = await requestJson('/Productos/nombres', { token, signal })
  if (!Array.isArray(result) || !result.every(item => item && Number.isSafeInteger(item.idProducto) && item.idProducto > 0 && typeof item.nombre === 'string') ||
    new Set(result.map(item => item.idProducto)).size !== result.length) throw new ApiError('Lista de nombres inválida.')
  return result.map(item => ({ idProducto: item.idProducto, nombre: item.nombre }))
}
export function filterProductNames(items: ProductSuggestion[], text: string): ProductSuggestion[] {
  const normalize = (value: string) => value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLocaleLowerCase('es').trim()
  const query = normalize(text)
  return query.length < 3 ? [] : items.filter(item => normalize(item.nombre).includes(query)).slice(0, 10)
}
