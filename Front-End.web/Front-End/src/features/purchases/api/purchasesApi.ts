import { requestJson } from '../../../shared/api/httpClient'
import { ApiError } from '../../../shared/api/ApiError'
import type { CreatePurchase, CreatedPurchase, Purchase, PurchaseDetail, Supplier } from '../types/purchase'

const object = (v: unknown): v is Record<string, unknown> => !!v && typeof v === 'object'
const id = (v: unknown): v is number => typeof v === 'number' && Number.isSafeInteger(v) && v > 0
const amount = (v: unknown): v is number => typeof v === 'number' && Number.isFinite(v) && v >= 0
const badResponse = () => new ApiError('La respuesta de compras no es válida.')

export async function listSuppliers(token: string, signal?: AbortSignal): Promise<Supplier[]> {
  const rows = await requestJson('/Compra/proveedores', { token, signal })
  if (!Array.isArray(rows) || !rows.every(row => object(row) && id(row.idProveedor) && typeof row.nombre === 'string') ||
      new Set(rows.map(row => row.idProveedor)).size !== rows.length) throw badResponse()
  return rows
}
export async function listPurchases(token: string, page: number, signal?: AbortSignal): Promise<Purchase[]> {
  const rows = await requestJson(`/Compra/listar?pagina=${page}&tamanoPagina=20`, { token, signal })
  if (!Array.isArray(rows) || rows.length > 20 || !rows.every(row => object(row) && id(row.id_compra) &&
      typeof row.nombre_usuario === 'string' && typeof row.nombre_proveedor === 'string' &&
      typeof row.descripcion_estado_compra === 'string' && typeof row.fecha_ingreso === 'string' &&
      Number.isFinite(Date.parse(row.fecha_ingreso)) && amount(row.total_compra) && amount(row.saldo_pendiente) &&
      row.saldo_pendiente <= row.total_compra)) throw badResponse()
  return rows
}
export async function getPurchaseDetails(token: string, purchaseId: number, signal?: AbortSignal): Promise<PurchaseDetail[]> {
  const rows = await requestJson(`/Compra/detalle/${purchaseId}`, { token, signal })
  if (!Array.isArray(rows) || !rows.every(row => object(row) && id(row.id_detalle_compra) &&
      typeof row.nombre_producto === 'string' && id(row.cantidad) && amount(row.subtotal))) throw badResponse()
  return rows
}
export async function createPurchase(token: string, body: CreatePurchase): Promise<CreatedPurchase> {
  const result = await requestJson('/Compra/crear', { method: 'POST', token, body, validationMessages: true })
  if (!object(result) || !id(result.id_compra) || !amount(result.total_compra) || !amount(result.saldo_pendiente) ||
      !Array.isArray(result.productos) || !result.productos.every(p => object(p) && id(p.idProducto) &&
      typeof p.stock === 'number' && Number.isSafeInteger(p.stock) && p.stock >= 0 && amount(p.precioPromedioCompra)))
    throw new ApiError('No se pudo confirmar la respuesta. Revisa el historial antes de registrar nuevamente.')
  return result as unknown as CreatedPurchase
}
export async function payPurchase(token: string, purchaseId: number, payment: number, note: string, operationId: string) {
  const result = await requestJson('/Compra/pago-compra', { method: 'POST', token, validationMessages: true,
    body: { idOperacion: operationId, id_compra: purchaseId, monto: payment, observacion: note.trim() || null } })
  if (!object(result) || !id(result.id_pago) || result.id_compra !== purchaseId || result.monto_pagado !== payment)
    throw new ApiError('No se pudo confirmar el abono. Revisa la compra antes de reintentar.')
}

export type PurchasePayment = { idPago: number; idCompra: number; fechaPago: string; monto: number | null; usuario: string; observacion: string | null }
export async function getPurchasePayments(token: string, purchaseId: number, signal?: AbortSignal): Promise<PurchasePayment[]> {
  const result = await requestJson('/Compra/' + purchaseId + '/pagos', { token, signal })
  if (!Array.isArray(result) || !result.every(p => object(p) && id(p.idPago) && p.idCompra === purchaseId &&
    typeof p.fechaPago === 'string' && !Number.isNaN(Date.parse(p.fechaPago)) &&
    (p.monto === null || (typeof p.monto === 'number' && Number.isFinite(p.monto) && p.monto > 0)) &&
    typeof p.usuario === 'string' && (p.observacion === null || typeof p.observacion === 'string')))
    throw new ApiError('El historial de pagos no tiene el formato esperado.')
  return result as PurchasePayment[]
}
