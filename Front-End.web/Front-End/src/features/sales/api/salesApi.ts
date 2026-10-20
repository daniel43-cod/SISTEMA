import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'
import type { SaleProduct } from '../utils/saleValidation'

export type SaleHistory = { id_ventas: number; cliente: string; total: number; ganancia_total?: number; saldo_pendiente: number; estado: string; fecha_venta: string }
const record = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null
const id = (v: unknown) => Number.isSafeInteger(v) && Number(v) > 0
const amount = (v: unknown) => typeof v === 'number' && Number.isFinite(v) && v >= 0

export async function loadSaleProducts(token: string, signal?: AbortSignal): Promise<SaleProduct[]> {
  const products: SaleProduct[] = []
  for (let page = 1; page <= 100000; page++) {
  const result = await requestJson(`/Venta/catalogo?pagina=${page}&tamanoPagina=100`, { token, signal })
  if (!Array.isArray(result) || !result.every(p => record(p) && id(p.id_producto) && typeof p.nombre === 'string' &&
    Number.isSafeInteger(p.stock) && Array.isArray(p.presentaciones) && p.presentaciones.every(v => record(v) &&
      id(v.id_producto_presentacion) && typeof v.presentacion === 'string' && id(v.unidades_equivalentes) && amount(v.precio))))
    throw new ApiError('El catálogo de ventas no tiene el formato esperado.')
  products.push(...result as SaleProduct[])
  if (result.length < 100) return products
  }
  throw new ApiError('El catálogo excede el límite de consulta.')
}
export async function loadSaleHistory(token: string, signal?: AbortSignal, page = 1): Promise<SaleHistory[]> {
  const result = await requestJson(`/Venta/listar?pagina=${page}&tamanoPagina=50`, { token, signal })
  if (!Array.isArray(result) || !result.every(v => record(v) && id(v.id_ventas) && typeof v.cliente === 'string' && amount(v.total) &&
    amount(v.saldo_pendiente) && (v.ganancia_total === undefined || (typeof v.ganancia_total === 'number' && Number.isFinite(v.ganancia_total))) && typeof v.estado === 'string' && typeof v.fecha_venta === 'string'))
    throw new ApiError('El historial de ventas no tiene el formato esperado.')
  return result as SaleHistory[]
}
export async function findBarcode(code: string, token: string, signal?: AbortSignal): Promise<SaleProduct> {
  const result = await requestJson(`/Productos/codigo/${encodeURIComponent(code.trim())}`, { token, signal, validationMessages: true })
  if (!record(result) || !id(result.id_producto) || typeof result.nombre_producto !== 'string' ||
      !Number.isSafeInteger(result.stock) || Number(result.stock) < 0 || !Array.isArray(result.presentaciones) ||
      !result.presentaciones.length || !result.presentaciones.every(p => record(p) && id(p.id_producto_presentacion) &&
        typeof p.presentacion === 'string' && id(p.unidades_equivalentes) && amount(p.precio) && Number(p.precio) > 0))
    throw new ApiError('Respuesta de código de barras inválida.')
  return { id_producto: Number(result.id_producto), nombre: result.nombre_producto, stock: Number(result.stock),
    presentaciones: result.presentaciones as SaleProduct['presentaciones'] }
}
export async function createSale(body: unknown, token: string) {
  // Un POST de venta nunca se reintenta automáticamente.
  const result = await requestJson('/Venta/crear', { method: 'POST', body, token })
  if (!record(result) || !id(result.id_venta) || !amount(result.total) || !amount(result.saldo_pendiente))
    throw new ApiError('No se pudo confirmar la respuesta de la venta.', 200)
  return { id: Number(result.id_venta), total: Number(result.total), saldo: Number(result.saldo_pendiente) }
}

export type SaleDetail = { id_producto: number; nombre_producto: string; descripcion_resentacion: string; cantidad: number; precio: number; descuento: number }
export async function loadSaleDetails(saleId: number, token: string, signal?: AbortSignal): Promise<SaleDetail[]> {
  const result = await requestJson('/DetalleVenta_/listar/' + saleId, { token, signal })
  if (!Array.isArray(result) || !result.every(v => record(v) && id(v.id_producto) && typeof v.nombre_producto === 'string' &&
    typeof v.descripcion_resentacion === 'string' && id(v.cantidad) && amount(v.precio) && amount(v.descuento)))
    throw new ApiError('El detalle de venta no tiene el formato esperado.')
  return result as SaleDetail[]
}
export async function paySale(saleId: number, monto: number, observacion: string, token: string) {
  const result = await requestJson('/Venta/AbonarVenta', { method: 'POST', token, validationMessages: true,
    body: { id_venta: saleId, monto, observacion } })
  if (!record(result) || !id(result.id_pago) || result.id_venta !== saleId || result.monto !== monto)
    throw new ApiError('No se pudo confirmar la respuesta del abono.', 200)
}
