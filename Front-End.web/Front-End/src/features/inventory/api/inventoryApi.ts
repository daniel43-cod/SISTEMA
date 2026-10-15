import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'

export type InventoryItem = {
  idProducto: number; codigoBarra: string | null; nombre: string; marca: string
  stock: number; precioPromedioCompra: number | null
}
export type InventoryPageData = {
  pagina: number; tamanoPagina: number; total: number; items: InventoryItem[]
}
function record(value: unknown): value is Record<string, unknown> {
  return value !== null && typeof value === 'object' && !Array.isArray(value)
}
export async function listInventory(page: number, text: string, token: string, signal: AbortSignal): Promise<InventoryPageData> {
  const params = new URLSearchParams({ pagina: String(page), tamanoPagina: '25' })
  if (text.trim()) params.set('texto', text.trim())
  const result = await requestJson(`/Productos/inventario?${params}`, { token, signal })
  if (!record(result) || result.pagina !== page || result.tamanoPagina !== 25 ||
    !Number.isSafeInteger(result.total) || Number(result.total) < 0 || !Array.isArray(result.items) ||
    result.items.length > 25 || result.items.length > Number(result.total) ||
    !result.items.every(item => record(item) && Number.isSafeInteger(item.idProducto) && Number(item.idProducto) > 0 &&
      (item.codigoBarra === null || typeof item.codigoBarra === 'string') &&
      typeof item.nombre === 'string' && typeof item.marca === 'string' && Number.isSafeInteger(item.stock) &&
      (item.precioPromedioCompra === null || (typeof item.precioPromedioCompra === 'number' &&
        Number.isFinite(item.precioPromedioCompra) && item.precioPromedioCompra >= 0))) ||
    new Set(result.items.map(item => item.idProducto)).size !== result.items.length)
    throw new ApiError('El servidor devolvió datos de inventario inválidos.')
  return result as InventoryPageData
}
