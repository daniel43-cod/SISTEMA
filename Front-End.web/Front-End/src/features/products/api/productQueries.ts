import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
import type { ProductPage, ProductDetail, ProductSummary } from '../types/product.ts'

function record(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}
function summary(value: unknown): value is ProductSummary & Record<string, unknown> {
  return record(value) && Number.isSafeInteger(value.idProducto) && Number(value.idProducto) > 0 &&
    typeof value.nombre === 'string' && typeof value.marca === 'string' && typeof value.categoria === 'string' &&
    typeof value.marcaActiva === 'boolean' && typeof value.categoriaActiva === 'boolean' &&
    Number.isSafeInteger(value.stockUnidades) && Number.isSafeInteger(value.stockMinimo) &&
    (value.codigoBarra === null || typeof value.codigoBarra === 'string')
}
function invalid(): never { throw new ApiError('El servidor devolvió datos de productos inválidos.') }

export async function listProducts(page: number, token: string, signal?: AbortSignal, filters: { idMarca?: number; idCategoria?: number } = {}): Promise<ProductPage> {
  if (!Number.isInteger(page) || page < 1 || page > 100000) throw new ApiError('Página inválida.', 400)
  for (const id of [filters.idMarca, filters.idCategoria])
    if (id !== undefined && (!Number.isSafeInteger(id) || id <= 0)) throw new ApiError('Filtro inválido.', 400)
  const params = new URLSearchParams({ pagina: String(page), tamanoPagina: '20' })
  if (filters.idMarca !== undefined) params.set('idMarca', String(filters.idMarca))
  if (filters.idCategoria !== undefined) params.set('idCategoria', String(filters.idCategoria))
  const result = await requestJson(`/Productos/listar?${params}`, { token, signal })
  if (!record(result) || result.pagina !== page || result.tamanoPagina !== 20 ||
    !Number.isSafeInteger(result.total) || Number(result.total) < 0 || !Array.isArray(result.items) ||
    result.items.length > 20 || result.items.length > Number(result.total) || !result.items.every(summary) ||
    new Set(result.items.map(item => item.idProducto)).size !== result.items.length) invalid()
  return result as ProductPage
}
export async function getProductDetail(id: number, token: string, signal?: AbortSignal): Promise<ProductDetail> {
  if (!Number.isSafeInteger(id) || id <= 0) throw new ApiError('Producto inválido.', 400)
  const result = await requestJson(`/Productos/${id}`, { token, signal })
  if (!summary(result) || result.idProducto !== id || !record(result) ||
    typeof result.fechaCreacion !== 'string' || !Array.isArray(result.presentaciones) ||
    !result.presentaciones.every(item => record(item) &&
      Number.isSafeInteger(item.idProductoPresentacion) && Number(item.idProductoPresentacion) > 0 &&
      (item.descripcion === null || typeof item.descripcion === 'string') &&
      Number.isSafeInteger(item.unidadesEquivalentes) && typeof item.precio === 'number' && Number.isFinite(item.precio) &&
      typeof item.activa === 'boolean' && typeof item.presentacionActiva === 'boolean' &&
      Number.isSafeInteger(item.presentacionesDisponibles) && Number(item.presentacionesDisponibles) >= 0)) invalid()
  return result as ProductDetail
}