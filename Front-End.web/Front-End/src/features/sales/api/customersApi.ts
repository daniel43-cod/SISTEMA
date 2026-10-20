import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type SaleCustomer = { id_cliente: number; nombre: string }
export type SaleCustomerDetail = SaleCustomer & {
  apellido: string | null; nit: string | null; dpi: string | null; telefono: string | null
  correo_electronico: string | null; direccion: string | null
}
const record = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null
const validId = (value: unknown) => Number.isSafeInteger(value) && Number(value) > 0

export async function loadSaleCustomers(token: string, signal?: AbortSignal): Promise<SaleCustomer[]> {
  const result = await requestJson('/Cliente/listar', { token, signal })
  if (!Array.isArray(result) || !result.every(c => record(c) && validId(c.id_cliente) && typeof c.nombre === 'string') ||
    new Set(result.map(c => c.id_cliente)).size !== result.length)
    throw new ApiError('La lista de clientes no tiene el formato esperado.')
  // Construir el índice explícitamente: solo se conserva ID + nombre en memoria.
  return result.map(c => ({ id_cliente: c.id_cliente as number, nombre: c.nombre as string }))
}

export async function loadSaleCustomer(id: number, token: string, signal?: AbortSignal): Promise<SaleCustomerDetail> {
  if (!validId(id)) throw new ApiError('Selecciona un cliente válido.', 400)
  const result = await requestJson(`/Cliente/${id}`, { token, signal })
  const fields = ['apellido', 'nit', 'dpi', 'telefono', 'correo_electronico', 'direccion'] as const
  if (!record(result) || result.id_cliente !== id || typeof result.nombre !== 'string' ||
    fields.some(field => result[field] !== null && typeof result[field] !== 'string'))
    throw new ApiError('Los datos del cliente no tienen el formato esperado.')
  return { id_cliente: id, nombre: result.nombre, apellido: result.apellido as string | null,
    nit: result.nit as string | null, dpi: result.dpi as string | null, telefono: result.telefono as string | null,
    correo_electronico: result.correo_electronico as string | null, direccion: result.direccion as string | null }
}
