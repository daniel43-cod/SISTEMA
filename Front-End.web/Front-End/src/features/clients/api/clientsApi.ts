import { requestJson } from '../../../shared/api/httpClient.ts'
import { ApiError } from '../../../shared/api/ApiError.ts'

export type Client = { idCliente: number; nombre: string; nit: string | null; telefono: string | null; estado: boolean; direccion: string | null }
export type ClientData = { id_cliente: number; nombre: string; apellido: string | null; nit: string | null; dpi: string | null; telefono: string | null; correo_electronico: string | null; direccion: string | null }
export type ClientSummary = { cliente: ClientData; estado: boolean; cantidadVentas: number; totalComprado: number; totalPagado: number; saldoPendiente: number }
export type ClientSale = { idVenta: number; fecha: string; idUsuario: number; responsable: string; nombreResponsable?: string; total: number; pagado: number; saldo: number; idEstadoVenta: number; estado: string }
export type ClientPage<T> = { totalRegistros: number; pagina: number; tamanoPagina: number; hayMas: boolean; items: T[] }
export type ClientSalesPage = ClientPage<ClientSale> & { idCliente: number }
const object = (v: unknown): v is Record<string, unknown> => v !== null && typeof v === 'object'
const id = (v: unknown): v is number => Number.isSafeInteger(v) && Number(v) > 0
const count = (v: unknown): v is number => Number.isSafeInteger(v) && Number(v) >= 0
const amount = (v: unknown): v is number => typeof v === 'number' && Number.isFinite(v) && v >= 0
const text = (v: unknown) => v === null || typeof v === 'string'
function invalid(): never { throw new ApiError('La respuesta de clientes no tiene el formato esperado.') }
function page(v: unknown): v is ClientPage<unknown> {
  return object(v) && count(v.totalRegistros) && id(v.pagina) && v.pagina <= 100000 && id(v.tamanoPagina) && v.tamanoPagina <= 100
    && typeof v.hayMas === 'boolean' && v.hayMas === (v.pagina * v.tamanoPagina < v.totalRegistros)
    && Array.isArray(v.items) && v.items.length <= v.tamanoPagina && v.items.length === Math.max(0, Math.min(v.tamanoPagina, v.totalRegistros - (v.pagina - 1) * v.tamanoPagina))
}
export function parseClients(v: unknown): ClientPage<Client> {
  if (!page(v) || !v.items.every(c => object(c) && id(c.idCliente) && typeof c.nombre === 'string' && text(c.nit) && text(c.telefono) && (c.direccion === undefined || text(c.direccion)) && typeof c.estado === 'boolean')
    || new Set(v.items.map(c => (c as Client).idCliente)).size !== v.items.length) invalid()
  return { ...v, items: v.items.map(c => ({ ...(c as Client), direccion: (c as Client).direccion ?? null })) } as ClientPage<Client>
}
export function parseClientSummary(v: unknown, expected: number): ClientSummary {
  if (!object(v) || !object(v.cliente) || v.cliente.id_cliente !== expected || typeof v.cliente.nombre !== 'string'
    || ['apellido', 'nit', 'dpi', 'telefono', 'correo_electronico', 'direccion'].some(k => !text((v.cliente as Record<string, unknown>)[k]))
    || typeof v.estado !== 'boolean' || !count(v.cantidadVentas) || !amount(v.totalComprado) || !amount(v.totalPagado) || !amount(v.saldoPendiente)) invalid()
  return v as ClientSummary
}
export function parseClientSales(v: unknown, expected: number): ClientSalesPage {
  if (!page(v) || !('idCliente' in v) || v.idCliente !== expected || !v.items.every(s => object(s) && id(s.idVenta) && id(s.idUsuario)
    && typeof s.responsable === 'string' && (s.nombreResponsable === undefined || typeof s.nombreResponsable === 'string') && typeof s.fecha === 'string' && Number.isFinite(Date.parse(s.fecha))
    && amount(s.total) && amount(s.pagado) && amount(s.saldo) && s.saldo <= s.total && s.pagado <= s.total && id(s.idEstadoVenta) && typeof s.estado === 'string')
    || new Set(v.items.map(s => (s as ClientSale).idVenta)).size !== v.items.length) invalid()
  return v as ClientSalesPage
}
export async function loadClients(token: string, pagina: number, buscar: string, signal?: AbortSignal) {
  const query = new URLSearchParams({ pagina: String(pagina), tamanoPagina: '30', buscar })
  return parseClients(await requestJson('/clientes?' + query, { token, signal }))
}
export async function loadClientSummary(token: string, clientId: number, signal?: AbortSignal) {
  return parseClientSummary(await requestJson(`/clientes/${clientId}/resumen`, { token, signal }), clientId)
}
export async function loadClientSales(token: string, clientId: number, pagina: number, signal?: AbortSignal) {
  return parseClientSales(await requestJson(`/clientes/${clientId}/ventas?pagina=${pagina}&tamanoPagina=30`, { token, signal }), clientId)
}
