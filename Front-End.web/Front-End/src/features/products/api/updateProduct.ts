import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'
export type EditProductPresentation = { id_producto_presentacion?: number; id_presentacion: number; unidades_equivalentes: number; precio: number; estado: boolean }
export type UpdateProductRequest = { codigo_barra: string; nombre: string; idMarca: number; stock_minimo: number; presentaciones?: EditProductPresentation[] }
const hasControl = (value: string) => Array.from(value).some(char => { const code = char.charCodeAt(0); return code < 32 || (code >= 127 && code <= 159) })
export async function updateProduct(id: number, data: UpdateProductRequest, token: string, signal?: AbortSignal) {
  const code = data.codigo_barra.trim(), name = data.nombre.trim()
  if (!Number.isSafeInteger(id) || id <= 0 || !code || code.length > 100 || /\s/.test(code) || hasControl(data.codigo_barra) ||
    !name || name.length > 200 || hasControl(data.nombre) || !Number.isSafeInteger(data.idMarca) || data.idMarca <= 0 ||
    !Number.isInteger(data.stock_minimo) || data.stock_minimo < 0 || data.stock_minimo > 2147483647)
    throw new ApiError('Revisa el nombre, código de barras, marca y existencia mínima.', 400)
  if (data.presentaciones) {
    const rows = data.presentaciones
    if (rows.length > 100 || new Set(rows.map(row => row.id_presentacion)).size !== rows.length ||
      new Set(rows.filter(row => row.id_producto_presentacion !== undefined).map(row => row.id_producto_presentacion)).size !== rows.filter(row => row.id_producto_presentacion !== undefined).length ||
      rows.some(row => !Number.isInteger(row.id_presentacion) || row.id_presentacion <= 0 ||
        (row.id_producto_presentacion !== undefined && (!Number.isInteger(row.id_producto_presentacion) || row.id_producto_presentacion <= 0)) ||
        !Number.isInteger(row.unidades_equivalentes) || row.unidades_equivalentes <= 0 || row.unidades_equivalentes > 2147483647 ||
        !Number.isFinite(row.precio) || row.precio <= 0 || row.precio > Number.MAX_SAFE_INTEGER / 100 || Math.abs(row.precio * 100 - Math.round(row.precio * 100)) > 0.000001 || typeof row.estado !== 'boolean'))
      throw new ApiError('Revisa las presentaciones: sin duplicados, unidades enteras positivas y precios de hasta dos decimales.', 400)
  }
  try {
    const result = await requestJson(`/Productos/${id}`, { method: 'PUT', token, signal,
      body: { codigo_barra: code, nombre: name, idMarca: data.idMarca, stock_minimo: data.stock_minimo, ...(data.presentaciones ? { presentaciones: data.presentaciones } : {}) } })
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